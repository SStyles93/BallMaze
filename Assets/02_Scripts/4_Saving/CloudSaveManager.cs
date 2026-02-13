using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;

#region PAYLOAD

[Serializable]
public class CloudSavePayload : SaveableData
{
    public string deviceId;
    public long version;

    public PlayerData player;
    public SkinShopData skinShop;
    public GameData game;
    public TutorialData tutorial;
}

#endregion

public class CloudSaveManager : MonoBehaviour
{
    public static CloudSaveManager Instance { get; private set; }

    // ==============================
    // CONFIG
    // ==============================

    private string CLOUD_KEY = "mm_cloud_save";

    [SerializeField] private bool verboseLogging = false;
    [SerializeField] private float saveIntervalHours = 10f; // save every 10 hours of gameplay

    [SerializeField] private float minSaveInterval = 600f; // 600 seconds (10min)
    private float _lastSaveTime = -999f;
    private bool _isSaving = false;
    [SerializeField] private float minDeleteInterval = 600f; // 600 seconds (10min)
    private float _lastDeleteTime = -999f;
    private bool _isDeleting = false;

    public bool IsAvailable => isAvailable;
    private bool isAvailable;
    private long lastKnownCloudVersion = -1;
    private float accumulatedPlayTime = 0f; // in seconds
    private bool isDirty = false;

    private const string LastSaveTime = "CloudSave_LastPlayTime";
    private const string LastPlayerIdKey = "Last_PlayerId";

    private readonly SemaphoreSlim saveSemaphore = new(1, 1);
    public Task InitializationTask => _initTcs.Task;
    private TaskCompletionSource<bool> _initTcs = new TaskCompletionSource<bool>();

    private CloudSavePayload currentPayload;

    // ==============================
    // EVENTS
    // ==============================

    public event Action OnCloudSaveCompleted;
    public event Action OnCloudLoadCompleted;
    public event Action<Exception> OnCloudOperationFailed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Load previous saved playtime if not loaded yet
        if (!PlayerPrefs.HasKey(LastSaveTime))
            PlayerPrefs.SetFloat(LastSaveTime, 0f);
    }

    private void OnEnable()
    {
        LoginManager.OnAuthenticationReady += OnAuthenticationReady;
    }

    private void OnDisable()
    {
        LoginManager.OnAuthenticationReady -= OnAuthenticationReady;
    }

    private void Update()
    {
        if (!IsAvailable) return;

        accumulatedPlayTime += Time.deltaTime;

        float lastSavedTime = PlayerPrefs.GetFloat(LastSaveTime, 0f);
        float totalTime = lastSavedTime + accumulatedPlayTime;

        if (totalTime >= saveIntervalHours * 3600f || isDirty)
        {
            TrySaveAllToCloud();
            if (verboseLogging)
                Debug.Log("[CloudSave] Auto Saving");
        }
    }

    private void OnApplicationQuit()
    {
        // --- SAVING OF PLAY TIME ---
        if (accumulatedPlayTime > 0f)
        {
            float lastSavedTime = PlayerPrefs.GetFloat(LastSaveTime, 0f);
            PlayerPrefs.SetFloat(LastSaveTime, lastSavedTime + accumulatedPlayTime);
            PlayerPrefs.Save();
        }
    }

    private async void OnAuthenticationReady()
    {
        isAvailable = true;

        string currentPlayerId = AuthenticationService.Instance.PlayerId;
        string lastPlayerId = PlayerPrefs.GetString(LastPlayerIdKey, "");

        bool isNewPlayer = currentPlayerId != lastPlayerId;

        if (verboseLogging)
        {
            Debug.Log($"[CloudSave] OnAuthenticationReady\n - PlayerId: {currentPlayerId}");
            Debug.Log($"[CloudSave] OnAuthenticationReady\n - NewPlayer: {isNewPlayer}");
        }

        if (isNewPlayer)
        {
            // Account changed on same device
            if (verboseLogging)
                Debug.Log("[CloudSave] OnAuthenticationReady\n - New account detected. Clearing local data.");

            SavingManager.Instance.DeleteAllData();
            lastKnownCloudVersion = -1;

            await ForceCloudLoad();
            OnCloudLoadCompleted?.Invoke();
        }
        else if (!SavingManager.Instance.isDataPresent)
        {
            if (verboseLogging)
                Debug.Log("[CloudSave] OnAuthenticationReady\n - No local data found. Forcing cloud load");

            await ForceCloudLoad();
            OnCloudLoadCompleted?.Invoke();
        }
        else
        {
            if (verboseLogging)
                Debug.Log("[CloudSave] OnAuthenticationReady\n - Same account + same device. Skipping cloud load.");
            OnCloudLoadCompleted?.Invoke();
        }


        // Save current identifiers
        PlayerPrefs.SetString(LastPlayerIdKey, currentPlayerId);
        PlayerPrefs.Save();

        _initTcs.TrySetResult(true);
    }


    // ==============================
    // PUBLIC ENTRY POINTS
    // ==============================

    public async void ForceCloudSave()
    {
        if (!IsAvailable)
        {
            if (verboseLogging)
                Debug.LogWarning("[CloudSave] ForceCloudSave\n - Cannot force save data: Cloud save not available.");
            return;
        }

        if (_isSaving)
        {
            if (verboseLogging)
                Debug.Log("[CloudSave] ForceCloudSave\n - Save blocked (already saving).");
            return;
        }

        // Prevent spam saving
        if (Time.time - _lastSaveTime < minSaveInterval)
        {
            if (verboseLogging)
                Debug.Log("[CloudSave] ForceCloudSave\n - Save blocked (cooldown active).");
            return;
        }

        _isSaving = true;
        _lastSaveTime = Time.time;

        try
        {
            SavingManager.Instance.SaveSession();

            await SaveWithConflictResolutionAsync();

            accumulatedPlayTime = 0f;
            PlayerPrefs.SetFloat(LastSaveTime, 0f);
            PlayerPrefs.Save();

            if (verboseLogging)
                Debug.Log("[CloudSave] ForceCloudSave\n - Forced cloud save and reset timer.");
        }
        finally
        {
            _isSaving = false;
        }

    }

    public async void ForceDeleteCloudData()
    {
        if (!IsAvailable)
        {
            if (verboseLogging)
                Debug.LogWarning("[CloudSave] ForceDeleteCloudData\n - Cannot force delete data: Cloud save not available.");
            return;
        }

        if (_isDeleting)
        {
            if (verboseLogging)
                Debug.Log("[CloudSave] ForceDeleteCloudData\n - Delete blocked (already deleting).");
            return;
        }

        // Prevent spam deleting
        if (Time.time - _lastDeleteTime < minDeleteInterval)
        {
            if (verboseLogging)
                Debug.Log("[CloudSave] ForceDeleteCloudData\n - Delete blocked (cooldown active).");
            return;
        }

        _isDeleting = true;
        _lastDeleteTime = Time.time;


        try
        {
            await CloudSaveService.Instance.Data.Player.DeleteAllAsync();

            accumulatedPlayTime = 0f;
            PlayerPrefs.SetFloat(LastSaveTime, 0f);
            PlayerPrefs.Save();

            SavingManager.Instance.DeleteAllData();

            Debug.Log("[CloudSave] ForceDeleteCloudData\n - Cloud data deleted successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSave] ForceDeleteCloudData\n - Failed to delete cloud data: {e}");
        }
        finally
        {
            _isDeleting = false;
        }
    }

    public void MarkDirty()
    {
        isDirty = true;
    }

    // ==============================
    // SAVE / LOAD PIPELINE
    // ==============================

    private void TrySaveAllToCloud()
    {
        if (!IsAvailable) return;

        if (verboseLogging)
            Debug.Log("[CloudSave] TrySaveAllToCloud");

        isDirty = false;
        PlayerPrefs.SetFloat(LastSaveTime, 0f);
        accumulatedPlayTime = 0f;

        _ = SaveWithConflictResolutionAsync();

        if (verboseLogging)
            Debug.Log("[CloudSave] TrySaveAllToCloud\n - Save completed and timers reset");
    }

    private async Task ForceCloudLoad()
    {
        try
        {
            CloudSavePayload cloudPayload = await LoadFromCloudAsync();

            if (cloudPayload != null)
            {
                ApplyCloudPayload(cloudPayload);
                lastKnownCloudVersion = cloudPayload.version;

                if (verboseLogging)
                    Debug.Log($"[CloudSave] ForceCloudLoad\n - Applied: v{cloudPayload.version}");
            }
            else
            {
                if (verboseLogging)
                    Debug.Log("[CloudSave] ForceCloudLoad\n - No cloud save found. Starting fresh.");
            }
        }
        catch (Exception e)
        {
            OnCloudOperationFailed?.Invoke(e);
            Debug.LogError($"[CloudSave] ForceCloudLoad\n - failed: {e}");
        }
    }

    private async Task SaveWithConflictResolutionAsync()
    {
        await saveSemaphore.WaitAsync();
        try
        {
            CloudSavePayload localPayload = currentPayload;
            CloudSavePayload cloudPayload = await LoadFromCloudAsync();

            if (cloudPayload != null)
            {
                if (cloudPayload.version > localPayload.version)
                {
                    ApplyCloudPayload(cloudPayload);
                    lastKnownCloudVersion = cloudPayload.version;

                    isDirty = false;
                    accumulatedPlayTime = 0f;
                    return;
                }

                if (localPayload.version == cloudPayload.version)
                    return;
            }

            await SaveToCloudAsync(localPayload);
            lastKnownCloudVersion = localPayload.version;

            OnCloudSaveCompleted?.Invoke();

            if (verboseLogging)
                Debug.Log($"[CloudSave] SaveWithConflictResolutionAsync\n - Saved: v{lastKnownCloudVersion}");
        }
        catch (Exception e)
        {
            OnCloudOperationFailed?.Invoke(e);
            Debug.LogError($"[CloudSave] SaveWithConflictResolutionAsync\n - Failed: {e}");
        }
        finally
        {
            saveSemaphore.Release();
        }
    }


    // ==============================
    // UNITY CLOUD - SAVE/LOAD
    // ==============================

    private async Task<CloudSavePayload> LoadFromCloudAsync()
    {
        var result = await CloudSaveService.Instance.Data.Player.LoadAsync(
            new HashSet<string> { CLOUD_KEY });

        if (result.TryGetValue(CLOUD_KEY, out var item))
        {
            CloudSavePayload payload = item.Value.GetAs<CloudSavePayload>();

            if (verboseLogging)
                Debug.Log($"[CloudSave] LoadedFromCloudAsyn\n - Loaded: v{payload.version}");

            return payload;
        }

        return null;
    }

    private async Task SaveToCloudAsync(CloudSavePayload payload)
    {
        var data = new Dictionary<string, object>
        {
            { CLOUD_KEY, payload }
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }


    // ==============================
    // CONFLICT RESOLUTION
    // ==============================

    private void ApplyCloudPayload(CloudSavePayload payload)
    {
        SavingManager.Instance.ForceLocalOverwrite(payload.game);
        SavingManager.Instance.ForceLocalOverwrite(payload.player);
        SavingManager.Instance.ForceLocalOverwrite(payload.skinShop);
        SavingManager.Instance.ForceLocalOverwrite(payload.tutorial);
    }

    public CloudSavePayload BuildPayload()
    {
        currentPayload = new CloudSavePayload
        {
            deviceId = SystemInfo.deviceUniqueIdentifier,
            version = lastKnownCloudVersion + 1,

            player = SavingManager.Instance.Get<PlayerData>(),
            skinShop = SavingManager.Instance.Get<SkinShopData>(),
            game = SavingManager.Instance.Get<GameData>(),
            tutorial = SavingManager.Instance.Get<TutorialData>()
        };

        lastKnownCloudVersion++;

        if(verboseLogging)
        Debug.Log($"[CloudSave] BuildPayload\n - Payload created with: v{currentPayload.version}");

        return currentPayload;
    }
}

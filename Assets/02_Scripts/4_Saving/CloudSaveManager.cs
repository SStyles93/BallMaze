using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.CloudSave;

#region PAYLOAD

[Serializable]
public class CloudSavePayload : SaveableData
{
    public string deviceId;
    public long version;
    public DateTime lastModifiedUtc;

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
    [SerializeField] private float saveIntervalHours = 2f; // save every 2 hours

    [SerializeField] private float minSaveInterval = 600f; // 600 seconds (10min)
    private float _lastSaveTime = -999f;
    private bool _isSaving = false;
    [SerializeField] private float minDeleteInterval = 600f; // 600 seconds (10min)
    private float _lastDeleteTime = -999f;
    private bool _isDeleting = false;



    public bool IsAvailable => isAvailable;
    private bool isAvailable;
    private long lastKnownCloudVersion = -1;
    private const string PlayerPrefKey = "CloudSave_LastPlayTime";
    private float accumulatedPlayTime = 0f; // in seconds
    private bool isDirty = false;


    private readonly SemaphoreSlim saveSemaphore = new(1, 1);

    // ==============================
    // EVENTS
    // ==============================

    public event Action OnCloudSaveCompleted;
    public event Action OnCloudLoadCompleted;
    public event Action<Exception> OnCloudOperationFailed;

    // ==============================
    // UNITY
    // ==============================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        LoginManager.OnAuthenticationReady += SetCloudSaveEnabled;
    }

    private void OnDisable()
    {
        LoginManager.OnAuthenticationReady -= SetCloudSaveEnabled;
    }

    private void Update()
    {
        if (!IsAvailable) return;

        // --- TIMED DATA SAVING ---

        // Increment accumulated play time in seconds
        accumulatedPlayTime += Time.deltaTime;

        // Load previous saved playtime if not loaded yet
        if (!PlayerPrefs.HasKey(PlayerPrefKey))
            PlayerPrefs.SetFloat(PlayerPrefKey, 0f);

        float lastSavedTime = PlayerPrefs.GetFloat(PlayerPrefKey, 0f);
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
            float lastSavedTime = PlayerPrefs.GetFloat(PlayerPrefKey, 0f);
            PlayerPrefs.SetFloat(PlayerPrefKey, lastSavedTime + accumulatedPlayTime);
            PlayerPrefs.Save();
        }
    }
    // ==============================
    // PUBLIC ENTRY POINTS
    // ==============================

    // --- User Buttons ---

    public async void ForceCloudSave()
    {
        if (!IsAvailable)
        {
            if (verboseLogging)
                Debug.LogWarning("[CloudSave] Cannot force save data: Cloud save not available.");
            return;
        }

        if (_isSaving)
        {
            if (verboseLogging)
                Debug.Log("[CloudSave] Save blocked (already saving).");
            return;
        }

        // Prevent spam saving
        if (Time.time - _lastSaveTime < minSaveInterval)
        {
            if (verboseLogging)
                Debug.Log("[CloudSave] Save blocked (cooldown active).");
            return;
        }

        _isSaving = true;
        _lastSaveTime = Time.time;

        try
        {
            SavingManager.Instance.SaveSession();

            await SaveWithConflictResolutionAsync();

            accumulatedPlayTime = 0f;
            PlayerPrefs.SetFloat(PlayerPrefKey, 0f);
            PlayerPrefs.Save();

            if (verboseLogging)
                Debug.Log("[CloudSave] Forced cloud save and reset timer.");
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
                Debug.LogWarning("[CloudSave] Cannot force delete data: Cloud save not available.");
            return;
        }

        if (_isDeleting)
        {
            if (verboseLogging)
                Debug.Log("[CloudSave] Delete blocked (already deleting).");
            return;
        }

        // Prevent spam deleting
        if (Time.time - _lastDeleteTime < minDeleteInterval)
        {
            if (verboseLogging)
                Debug.Log("[CloudSave] Delete blocked (cooldown active).");
            return;
        }

        _isDeleting = true;
        _lastSaveTime = Time.time;


        try
        {
            await CloudSaveService.Instance.Data.Player.DeleteAllAsync();

            accumulatedPlayTime = 0f;
            PlayerPrefs.SetFloat(PlayerPrefKey, 0f);
            PlayerPrefs.Save();

            SavingManager.Instance.DeleteAllData();

            Debug.Log("[CloudSave] Cloud data deleted successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSave] Failed to delete cloud data: {e}");
        }
        finally
        {
            _isDeleting = false;
        }
    }

    // --- API ENTRY ---

    public void MarkDirty()
    {
        isDirty = true;
    }

    public void TrySaveAllToCloud()
    {
        if (!IsAvailable) return;

        if (verboseLogging)
            Debug.Log("[CloudSave] Trying to Save All Data to Cloud");

        isDirty = false;
        PlayerPrefs.SetFloat(PlayerPrefKey, 0f);
        accumulatedPlayTime = 0f;

        _ = SaveWithConflictResolutionAsync();

        if (verboseLogging)
            Debug.Log("[CloudSave] Save completed and timers reset");
    }

    public void TryLoadAllFromCloud()
    {
        if (!IsAvailable) return;

        if (verboseLogging)
            Debug.Log("[CloudSave] Trying to Load All Data from Cloud");

        _ = LoadAndResolveAsync();
    }

    // ==============================
    // PAYLOAD BUILDING
    // ==============================

    private CloudSavePayload BuildPayload()
    {
        return new CloudSavePayload
        {
            deviceId = SystemInfo.deviceUniqueIdentifier,
            version = lastKnownCloudVersion + 1,
            lastModifiedUtc = DateTime.UtcNow,

            player = SavingManager.Instance.Get<PlayerData>(),
            skinShop = SavingManager.Instance.Get<SkinShopData>(),
            game = SavingManager.Instance.Get<GameData>(),
            tutorial = SavingManager.Instance.Get<TutorialData>()
        };
    }

    // ==============================
    // SAVE / LOAD PIPELINE
    // ==============================

    private async Task SaveWithConflictResolutionAsync()
    {
        await saveSemaphore.WaitAsync();
        try
        {
            CloudSavePayload localPayload = BuildPayload();
            CloudSavePayload cloudPayload = await LoadFromCloudAsync();

            CloudSavePayload winner = ResolveConflict(localPayload, cloudPayload);
            Debug.Log($"Saving resolved with payload v{winner.version} at {winner.lastModifiedUtc}");

            await SaveToCloudAsync(winner);

            lastKnownCloudVersion = winner.version;

            OnCloudSaveCompleted?.Invoke();

            if (verboseLogging)
                Debug.Log($"[CloudSave] Saved v{winner.version}");
        }
        catch (Exception e)
        {
            OnCloudOperationFailed?.Invoke(e);
            Debug.LogError($"[CloudSave] Save failed: {e}");
        }
        finally
        {
            saveSemaphore.Release();
        }
    }

    private async Task LoadAndResolveAsync()
    {
        try
        {
            CloudSavePayload cloudPayload = await LoadFromCloudAsync();
            if (cloudPayload == null)
            {
                Exception ex = new Exception($"[CloudSave] cloudPayload is null");
                OnCloudOperationFailed?.Invoke(ex);
                return;
            }

            CloudSavePayload localPayload = BuildPayload();

            CloudSavePayload winner = ResolveConflict(localPayload, cloudPayload);

            Debug.Log($"Loading resolved with payload v{winner.version} at {winner.lastModifiedUtc}");

            if (winner == cloudPayload)
            {
                ApplyCloudPayload(cloudPayload);
            }

            lastKnownCloudVersion = winner.version;

            OnCloudLoadCompleted?.Invoke();
        }
        catch (Exception e)
        {
            OnCloudOperationFailed?.Invoke(e);
            Debug.LogError($"[CloudSave] Load failed: {e}");
        }
    }

    // ==============================
    // CLOUD SDK
    // ==============================

    private void SetCloudSaveEnabled()
    {
        isAvailable = true;

        if (verboseLogging)
            Debug.Log($"[CloudSave] Enabled");
    }


    private async Task<CloudSavePayload> LoadFromCloudAsync()
    {
        var result = await CloudSaveService.Instance.Data.Player.LoadAsync(
            new HashSet<string> { CLOUD_KEY });

        if (result.TryGetValue(CLOUD_KEY, out var item))
        {
            CloudSavePayload payload = item.Value.GetAs<CloudSavePayload>();

            if (verboseLogging)
                Debug.Log($"[CloudSave] Loaded v{payload.version}");

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

    private CloudSavePayload ResolveConflict(
        CloudSavePayload local,
        CloudSavePayload cloud)
    {
        if (cloud == null) return local;
        if (local == null) return cloud;

        if (local.version != cloud.version)
            return local.version > cloud.version ? local : cloud;

        if (local.lastModifiedUtc != cloud.lastModifiedUtc)
            return local.lastModifiedUtc > cloud.lastModifiedUtc ? local : cloud;

        return local.deviceId == SystemInfo.deviceUniqueIdentifier ? local : cloud;
    }

    // ==============================
    // APPLY
    // ==============================

    private void ApplyCloudPayload(CloudSavePayload payload)
    {
        SavingManager.Instance.ForceLocalOverwrite(payload.game);
        SavingManager.Instance.ForceLocalOverwrite(payload.player);
        SavingManager.Instance.ForceLocalOverwrite(payload.skinShop);
        SavingManager.Instance.ForceLocalOverwrite(payload.tutorial);
    }
}

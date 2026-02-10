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

    public bool IsAvailable => isAvailable;
    private bool isAvailable;
    private long lastKnownCloudVersion = -1;
    private float _lastSaveTime = -999f;
    private const float SaveCooldownSeconds = 20f;

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

    private void SetCloudSaveEnabled()
    {
        isAvailable = true;

        if (verboseLogging)
            Debug.Log($"[CloudSave] Enabled");
    }

    // ==============================
    // PUBLIC ENTRY POINTS
    // ==============================

    public void TrySaveAllToCloud()
    {
        if (!IsAvailable) return;

        float now = Time.realtimeSinceStartup;
        if (now - _lastSaveTime < SaveCooldownSeconds)
            return;

        _lastSaveTime = now;

        _ = SaveWithConflictResolutionAsync();
    }

    public void TryLoadAllFromCloud()
    {
        if (!IsAvailable) return;
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
            if (cloudPayload == null) return;

            CloudSavePayload localPayload = BuildPayload();

            CloudSavePayload winner = ResolveConflict(localPayload, cloudPayload);

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

using UnityEngine;
using System.Collections;
using Google.Play.AppUpdate;
using Google.Play.Common;

public class GoogleUpdateManager : MonoBehaviour
{
    public static GoogleUpdateManager Instance { get; private set; }

    private AppUpdateManager _appUpdateManager;
    private AppUpdateInfo _appUpdateInfo;

    [SerializeField] private bool useImmediateUpdate = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _appUpdateManager = new AppUpdateManager();
    }

    public void CheckForUpdate()
    {
        StartCoroutine(CheckForUpdateRoutine());
    }

    private IEnumerator CheckForUpdateRoutine()
    {
        if (Application.isEditor)
            yield break;

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("No internet. Skipping update check.");
            yield break;
        }

        var infoOperation = _appUpdateManager.GetAppUpdateInfo();
        yield return infoOperation;

        if (!infoOperation.IsSuccessful)
        {
            Debug.LogWarning("Update check failed: " + infoOperation.Error);
            yield break;
        }

        _appUpdateInfo = infoOperation.GetResult();

        if (_appUpdateInfo.UpdateAvailability != UpdateAvailability.UpdateAvailable)
        {
            yield break;
        }

        AppUpdateOptions options = useImmediateUpdate
            ? AppUpdateOptions.ImmediateAppUpdateOptions()
            : AppUpdateOptions.FlexibleAppUpdateOptions();

        if (!_appUpdateInfo.IsUpdateTypeAllowed(options))
        {
            yield break;
        }


        _appUpdateManager.StartUpdate(_appUpdateInfo, options);
        if (!useImmediateUpdate)
        {
            StartCoroutine(MonitorFlexibleUpdate());
        }
    }

    private IEnumerator MonitorFlexibleUpdate()
    {
        while (true)
        {
            var infoOperation = _appUpdateManager.GetAppUpdateInfo();
            yield return infoOperation;

            if (!infoOperation.IsSuccessful)
                yield break;

            var info = infoOperation.GetResult();

            if (info.AppUpdateStatus == AppUpdateStatus.Downloaded)
            {
                _appUpdateManager.CompleteUpdate();
                yield break;
            }

            yield return new WaitForSeconds(1f);
        }
    }
}

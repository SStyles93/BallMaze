using Google.Play.Review;
using System.Collections;
using UnityEngine;

public class GoogleReviewManager : MonoBehaviour
{
    public static GoogleReviewManager Instance { get; private set; }

    [SerializeField] private bool verboseLogging = false;


    private ReviewManager _reviewManager;
    private PlayReviewInfo _reviewInfo;
    // process guard
    private bool _isProcessing;

    // timing
    private const string TotalPlayTimeKey = "TotalPlayTime";
    private const string ReviewStepKey = "ReviewStepIndex";
    private const string NextReviewTimeKey = "NextReviewTime";
    private float _sessionStartTime;

    private void Awake()
    {
        // Scene-based singleton (no DontDestroyOnLoad)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _reviewManager = new ReviewManager();
    }

    private void Start()
    {
        _sessionStartTime = Time.realtimeSinceStartup;

        if (!PlayerPrefs.HasKey(NextReviewTimeKey))
        {
            PlayerPrefs.SetFloat(NextReviewTimeKey, 600f); // First review at 10 min
            PlayerPrefs.SetInt(ReviewStepKey, 0);
            PlayerPrefs.Save();
        }
    }



    private void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveSessionTime();
    }

    private void OnApplicationQuit()
    {
        SaveSessionTime();
    }

    private void SaveSessionTime()
    {
        float sessionTime = Time.realtimeSinceStartup - _sessionStartTime;
        float totalTime = PlayerPrefs.GetFloat(TotalPlayTimeKey, 0f);
        totalTime += sessionTime;

        PlayerPrefs.SetFloat(TotalPlayTimeKey, totalTime);
        PlayerPrefs.Save();
    }

    [ContextMenu("Delete Review Prefs.")]
    public void DeleteReviewPlayerPrefs()
    {
        PlayerPrefs.DeleteKey(TotalPlayTimeKey);
        PlayerPrefs.DeleteKey(ReviewStepKey);
        PlayerPrefs.DeleteKey(NextReviewTimeKey);
    }

    /// <summary>
    /// Calls the Google Review API<br/>
    /// </summary>
    /// <remarks>Fires a request if: !processing, perfect level completed, enough time has passed</remarks>
    public void RequestReview()
    {
        if (_isProcessing || !CanRequestReview())
            return;

        StartCoroutine(RequestReviewCoroutine());
    }

    private bool CanRequestReview()
    {
        float totalTime = PlayerPrefs.GetFloat(TotalPlayTimeKey, 0f);
        float nextReviewTime = PlayerPrefs.GetFloat(NextReviewTimeKey, 600f); // 10 min default
        int step = PlayerPrefs.GetInt(ReviewStepKey, 0);

        if (totalTime < nextReviewTime)
        {
            if (verboseLogging)
                Debug.Log($"[ReviewManager] CanRequestReview\n - False with: TotalTime {totalTime} < {nextReviewTime}");
            return false;
        }

        if (verboseLogging)
            Debug.Log($"[ReviewManager] CanRequestReview\n - True with: TotalTime {totalTime} >= {nextReviewTime}");

        // Prepare next threshold (exponential growth)
        step++;
        float newThreshold = 600f * Mathf.Pow(2, step); // 10m * 2^step

        PlayerPrefs.SetInt(ReviewStepKey, step);
        PlayerPrefs.SetFloat(NextReviewTimeKey, newThreshold);
        PlayerPrefs.Save();

        return true;
    }

    private IEnumerator RequestReviewCoroutine()
    {
        if (verboseLogging)
            Debug.Log("[ReviewManager] RequestReviewCoroutine\n - Request Started");

        _isProcessing = true;

        // --- REQUEST REVIEW ---
        var requestFlowOperation = _reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;

        if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        {
            Debug.Log("Review Flow Request Error: " + requestFlowOperation.Error);
            _isProcessing = false;
            yield break;
        }

        _reviewInfo = requestFlowOperation.GetResult();

        // --- LAUNCH REVIEW ---
        var launchFlowOperation = _reviewManager.LaunchReviewFlow(_reviewInfo);
        yield return launchFlowOperation;

        _reviewInfo = null;
        _isProcessing = false;

        if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        {
            Debug.Log("Review Launch Error: " + launchFlowOperation.Error);
            yield break;
        }

        // Finality is in Google's control (outcome unpredictable)
    }
}

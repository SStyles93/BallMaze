using Google.Play.Review;
using System.Collections;
using UnityEngine;

public class GoogleReviewManager : MonoBehaviour
{
    public static GoogleReviewManager Instance { get; private set; }

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
            return false;

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

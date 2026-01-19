using Unity.Services.LevelPlay;
using UnityEngine;
using TMPro;
using MyBox;

public class AdsManager : MonoBehaviour
{
    #region Singleton
    public static AdsManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject);
    }
    #endregion

    [Header("Core Scene")]
    [SerializeField] private RectTransform adsArea;

    public LevelPlayBannerAd BannerAd;
    public LevelPlayInterstitialAd InterstitialAd;
    public LevelPlayRewardedAd RewardedVideoAd;

    [SerializeField] private TMP_Text adsDebugText;
    [SerializeField] private bool isDebug = false;
    [ConditionalField("isDebug")][SerializeField] private int debugPixelHeight;

    // -------- Banner layout authority --------
    public static int CurrentBannerHeightPx { get; private set; }
    public static event System.Action<int> OnBannerHeightChanged;

    private void Start()
    {
        LevelPlay.ValidateIntegration();

        LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed += SdkInitializationFailedEvent;

        LevelPlay.Init(AdConfig.AppKey);
    }

    private void Update()
    {
        if (isDebug)
        {
            CurrentBannerHeightPx = debugPixelHeight;
            ApplyBannerAreaSize();
            OnBannerHeightChanged?.Invoke(debugPixelHeight);
        }
    }

    #region SDK Init

    private void SdkInitializationCompletedEvent(LevelPlayConfiguration config)
    {
        Debug.Log($"[AdsManager] SDK initialized");

        EnableAds();
        LoadBanner();
    }

    private void SdkInitializationFailedEvent(LevelPlayInitError error)
    {
        Debug.LogError($"[AdsManager] SDK init failed: {error}");
    }

    #endregion

    #region Enable Ads

    private void EnableAds()
    {
        LevelPlay.OnImpressionDataReady += ImpressionDataReadyEvent;

        // ---------- Rewarded ----------
        RewardedVideoAd = new LevelPlayRewardedAd(AdConfig.RewardedVideoAdUnitId);

        RewardedVideoAd.OnAdLoaded += RewardedVideoOnLoadedEvent;
        RewardedVideoAd.OnAdLoadFailed += RewardedVideoOnAdLoadFailedEvent;
        RewardedVideoAd.OnAdDisplayed += RewardedVideoOnAdDisplayedEvent;
        RewardedVideoAd.OnAdDisplayFailed += RewardedVideoOnAdDisplayedFailedEvent;
        RewardedVideoAd.OnAdRewarded += RewardedVideoOnAdRewardedEvent;
        RewardedVideoAd.OnAdClicked += RewardedVideoOnAdClickedEvent;
        RewardedVideoAd.OnAdClosed += RewardedVideoOnAdClosedEvent;

        RewardedVideoAd.LoadAd();

        // ---------- Interstitial ----------
        InterstitialAd = new LevelPlayInterstitialAd(AdConfig.InterstitalAdUnitId);

        InterstitialAd.OnAdLoaded += InterstitialOnAdLoadedEvent;
        InterstitialAd.OnAdLoadFailed += InterstitialOnAdLoadFailedEvent;
        InterstitialAd.OnAdDisplayed += InterstitialOnAdDisplayedEvent;
        InterstitialAd.OnAdDisplayFailed += InterstitialOnAdDisplayFailedEvent;
        InterstitialAd.OnAdClosed += InterstitialOnAdClosedEvent;

        InterstitialAd.LoadAd();
    }

    #endregion

    #region Banner

    private void LoadBanner()
    {
        var configBuilder = new LevelPlayBannerAd.Config.Builder();

        configBuilder.SetSize(LevelPlayAdSize.BANNER);
        configBuilder.SetPosition(LevelPlayBannerPosition.TopCenter);
        configBuilder.SetDisplayOnLoad(true);
        configBuilder.SetRespectSafeArea(true);

        BannerAd = new LevelPlayBannerAd(
            AdConfig.BannerAdUnitId,
            configBuilder.Build()
        );

        BannerAd.OnAdLoaded += BannerOnAdLoadedEvent;
        BannerAd.OnAdLoadFailed += BannerOnAdLoadFailedEvent;
        //BannerAd.OnAdDisplayed += BannerOnAdDisplayedEvent;
        BannerAd.OnAdDisplayFailed += BannerOnAdDisplayedFailedEvent;

        BannerAd.LoadAd();
    }

    private int GetBannerHeightPx()
    {
        const float bannerDp = 50f; // Standard banner
        return Mathf.RoundToInt(bannerDp * (Screen.dpi / 160f));
    }

    #endregion

    #region Banner Callbacks

    private void BannerOnAdLoadedEvent(LevelPlayAdInfo adInfo)
    {
        CurrentBannerHeightPx = GetBannerHeightPx();

        ApplyBannerAreaSize();
        OnBannerHeightChanged?.Invoke(CurrentBannerHeightPx);

        adsDebugText.text = $"[AdsManager] Banner loaded ({CurrentBannerHeightPx}px)";
        //Debug.Log($"[AdsManager] Banner loaded ({CurrentBannerHeightPx}px)");
    }

    private void BannerOnAdLoadFailedEvent(LevelPlayAdError error)
    {
        //CurrentBannerHeightPx = 0;

        //ApplyBannerAreaSize();
        //OnBannerHeightChanged?.Invoke(0);

        adsDebugText.text = "[AdsManager] Banner failed to load";
        Debug.LogWarning("[AdsManager] Banner failed to load");
    }


    //private void BannerOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
    //{
    //    CurrentBannerHeightPx = GetBannerHeightPx();

    //    ApplyBannerAreaSize();
    //    OnBannerHeightChanged?.Invoke(CurrentBannerHeightPx);

    //    Debug.Log($"[AdsManager] Banner Displayed with ({CurrentBannerHeightPx}px)");
    //}

    private void BannerOnAdDisplayedFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        CurrentBannerHeightPx = 0;

        ApplyBannerAreaSize();
        OnBannerHeightChanged?.Invoke(0);

        adsDebugText.text = "[AdsManager] Banner failed to Display";
        Debug.LogWarning($"[AdsManager] Banner failed to display");
    }


    // Scales the Ads Area (Core Canvas)
    private void ApplyBannerAreaSize()
    {
        if (adsArea == null)
            return;

        float scaleFactor = adsArea.GetComponentInParent<Canvas>().scaleFactor;

        // Calculate top safe area in pixels
        float safeAreaTop = Screen.height - Screen.safeArea.yMax;

        // Total height = banner + safe area
        float totalHeight = (CurrentBannerHeightPx + safeAreaTop) / scaleFactor;

        adsArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
    }

    #endregion

    #region Rewarded Callbacks

    void RewardedVideoOnLoadedEvent(LevelPlayAdInfo adInfo) { }
    void RewardedVideoOnAdLoadFailedEvent(LevelPlayAdError error) { }
    void RewardedVideoOnAdDisplayedEvent(LevelPlayAdInfo adInfo) { }
    void RewardedVideoOnAdDisplayedFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error) { }
    void RewardedVideoOnAdRewardedEvent(LevelPlayAdInfo adInfo, LevelPlayReward reward) { }
    void RewardedVideoOnAdClickedEvent(LevelPlayAdInfo adInfo) { }
    void RewardedVideoOnAdClosedEvent(LevelPlayAdInfo adInfo) { }

    #endregion

    #region Interstitial Callbacks

    void InterstitialOnAdLoadedEvent(LevelPlayAdInfo adInfo) { }
    void InterstitialOnAdLoadFailedEvent(LevelPlayAdError error) { }
    void InterstitialOnAdDisplayedEvent(LevelPlayAdInfo adInfo) { }
    void InterstitialOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error) { }

    void InterstitialOnAdClosedEvent(LevelPlayAdInfo adInfo)
    {
        InterstitialAd.LoadAd();
    }

    #endregion

    private void ImpressionDataReadyEvent(LevelPlayImpressionData impressionData) { }

    private void OnDisable()
    {
        BannerAd?.DestroyAd();
        InterstitialAd?.DestroyAd();
        RewardedVideoAd?.DestroyAd();
    }
}

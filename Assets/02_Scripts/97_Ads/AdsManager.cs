using Unity.Services.LevelPlay;
using UnityEngine;


public class AdsManager : MonoBehaviour
{
    #region Singleton
    public static AdsManager Instance;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }
    #endregion

    public LevelPlayBannerAd BannerAd;
    public LevelPlayInterstitialAd InterstitialAd;
    public LevelPlayRewardedAd RewardedVideoAd;

    bool isAdsEnabled = false;

    public void Start()
    {
        Debug.Log("[LevelPlaySample] LevelPlay.ValidateIntegration");
        LevelPlay.ValidateIntegration();

        Debug.Log($"[LevelPlaySample] Unity version {LevelPlay.UnityVersion}");

        Debug.Log("[LevelPlaySample] Register initialization callbacks");
        LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed += SdkInitializationFailedEvent;

        // SDK init
        Debug.Log("[LevelPlaySample] LevelPlay SDK initialization");
        LevelPlay.Init(AdConfig.AppKey);

        LoadBanner();
        BannerAd.ShowAd();
    }

    public void LoadBanner()
    {
        var configBuilder = new LevelPlayBannerAd.Config.Builder();
        configBuilder.SetSize(LevelPlayAdSize.BANNER);
        configBuilder.SetPosition(LevelPlayBannerPosition.BottomCenter);
        configBuilder.SetDisplayOnLoad(true);
        configBuilder.SetRespectSafeArea(true); // Only relevant for Android
        //configBuilder.SetPlacementName("bannerPlacement");
        //configBuilder.SetBidFloor(1.0); // Minimum bid price in USD
        var bannerConfig = configBuilder.Build();

        BannerAd = new LevelPlayBannerAd(AdConfig.BannerAdUnitId, bannerConfig);
        BannerAd.LoadAd();
    }

    public void LoadTopBanner()
    {
        var configBuilder = new LevelPlayBannerAd.Config.Builder();
        configBuilder.SetSize(LevelPlayAdSize.BANNER);
        configBuilder.SetPosition(LevelPlayBannerPosition.TopCenter);
        configBuilder.SetDisplayOnLoad(true);
        configBuilder.SetRespectSafeArea(true); // Only relevant for Android
        //configBuilder.SetPlacementName("bannerPlacement");
        //configBuilder.SetBidFloor(1.0); // Minimum bid price in USD
        var bannerConfig = configBuilder.Build();

        BannerAd = new LevelPlayBannerAd(AdConfig.BannerAdUnitId, bannerConfig);
        BannerAd.LoadAd();
    }


    void EnableAds()
    {
        // Register to ImpressionDataReadyEvent
        LevelPlay.OnImpressionDataReady += ImpressionDataReadyEvent;

        // Create Rewarded Video object
        RewardedVideoAd = new LevelPlayRewardedAd(AdConfig.RewardedVideoAdUnitId);

        // Register to Rewarded Video events
        //RewardedVideoAd.OnAdLoaded += RewardedVideoOnLoadedEvent;
        //RewardedVideoAd.OnAdLoadFailed += RewardedVideoOnAdLoadFailedEvent;
        //RewardedVideoAd.OnAdDisplayed += RewardedVideoOnAdDisplayedEvent;
        //RewardedVideoAd.OnAdDisplayFailed += RewardedVideoOnAdDisplayedFailedEvent;
        //RewardedVideoAd.OnAdRewarded += RewardedVideoOnAdRewardedEvent;
        //RewardedVideoAd.OnAdClicked += RewardedVideoOnAdClickedEvent;
        //RewardedVideoAd.OnAdClosed += RewardedVideoOnAdClosedEvent;
        //RewardedVideoAd.OnAdInfoChanged += RewardedVideoOnAdInfoChangedEvent;

        //// Create Banner object
        //BannerAd = new LevelPlayBannerAd(AdConfig.BannerAdUnitId);

        //// Register to Banner events
        //BannerAd.OnAdLoaded += BannerOnAdLoadedEvent;
        //BannerAd.OnAdLoadFailed += BannerOnAdLoadFailedEvent;
        //BannerAd.OnAdDisplayed += BannerOnAdDisplayedEvent;
        //BannerAd.OnAdDisplayFailed += BannerOnAdDisplayFailedEvent;
        //BannerAd.OnAdClicked += BannerOnAdClickedEvent;
        //BannerAd.OnAdCollapsed += BannerOnAdCollapsedEvent;
        //BannerAd.OnAdLeftApplication += BannerOnAdLeftApplicationEvent;
        //BannerAd.OnAdExpanded += BannerOnAdExpandedEvent;

        //// Create Interstitial object
        //InterstitialAd = new LevelPlayInterstitialAd(AdConfig.InterstitalAdUnitId);

        //// Register to Interstitial events
        //InterstitialAd.OnAdLoaded += InterstitialOnAdLoadedEvent;
        //InterstitialAd.OnAdLoadFailed += InterstitialOnAdLoadFailedEvent;
        //InterstitialAd.OnAdDisplayed += InterstitialOnAdDisplayedEvent;
        //InterstitialAd.OnAdDisplayFailed += InterstitialOnAdDisplayFailedEvent;
        //InterstitialAd.OnAdClicked += InterstitialOnAdClickedEvent;
        //InterstitialAd.OnAdClosed += InterstitialOnAdClosedEvent;
        //InterstitialAd.OnAdInfoChanged += InterstitialOnAdInfoChangedEvent;
    }

    void SdkInitializationCompletedEvent(LevelPlayConfiguration config)
    {
        Debug.Log($"[LevelPlaySample] Received SdkInitializationCompletedEvent with Config: {config}");
        EnableAds();
        isAdsEnabled = true;
    }

    void SdkInitializationFailedEvent(LevelPlayInitError error)
    {
        Debug.Log($"[LevelPlaySample] Received SdkInitializationFailedEvent with Error: {error}");
    }

    void RewardedVideoOnLoadedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnLoadedEvent With AdInfo: {adInfo}");
    }

    void RewardedVideoOnAdLoadFailedEvent(LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdLoadFailedEvent With Error: {error}");
    }

    void RewardedVideoOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdDisplayedEvent With AdInfo: {adInfo}");
    }

    void RewardedVideoOnAdDisplayedFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdDisplayedFailedEvent With AdInfo: {adInfo} and Error: {error}");
    }

    void RewardedVideoOnAdRewardedEvent(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdRewardedEvent With AdInfo: {adInfo} and Reward: {reward}");
    }

    void RewardedVideoOnAdClickedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdClickedEvent With AdInfo: {adInfo}");
    }

    void RewardedVideoOnAdClosedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdClosedEvent With AdInfo: {adInfo}");
    }

    void RewardedVideoOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdInfoChangedEvent With AdInfo {adInfo}");
    }

    void InterstitialOnAdLoadedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdLoadedEvent With AdInfo: {adInfo}");
    }

    void InterstitialOnAdLoadFailedEvent(LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdLoadFailedEvent With Error: {error}");
    }

    void InterstitialOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdDisplayedEvent With AdInfo: {adInfo}");
    }

    void InterstitialOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdDisplayFailedEvent With AdInfo: {adInfo} and Error: {error}");
    }

    void InterstitialOnAdClickedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdClickedEvent With AdInfo: {adInfo}");
    }

    void InterstitialOnAdClosedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdClosedEvent With AdInfo: {adInfo}");
    }

    void InterstitialOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdInfoChangedEvent With AdInfo: {adInfo}");
    }

    void BannerOnAdLoadedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdLoadedEvent With AdInfo: {adInfo}");
    }

    void BannerOnAdLoadFailedEvent(LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdLoadFailedEvent With Error: {error}");
    }

    void BannerOnAdClickedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdClickedEvent With AdInfo: {adInfo}");
    }

    void BannerOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdDisplayedEvent With AdInfo: {adInfo}");
    }

    void BannerOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdDisplayFailedEvent With AdInfo: {adInfo} and Error: {error}");
    }

    void BannerOnAdCollapsedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdCollapsedEvent With AdInfo: {adInfo}");
    }

    void BannerOnAdLeftApplicationEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdLeftApplicationEvent With AdInfo: {adInfo}");
    }

    void BannerOnAdExpandedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdExpandedEvent With AdInfo: {adInfo}");
    }

    void ImpressionDataReadyEvent(LevelPlayImpressionData impressionData)
    {
        Debug.Log($"[LevelPlaySample] Received ImpressionDataReadyEvent ToString(): {impressionData}");
        Debug.Log($"[LevelPlaySample] Received ImpressionDataReadyEvent allData: {impressionData.AllData}");
    }

    private void OnDisable()
    {
        BannerAd?.DestroyAd();
        InterstitialAd?.DestroyAd();
    }
}

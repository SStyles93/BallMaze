using Unity.Services.LevelPlay;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    public void Start()
    {
        //// Register OnInitFailed and OnInitSuccess listeners
        //LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
        //LevelPlay.OnInitFailed += SdkInitializationFailedEvent;
        //// SDK init
        //LevelPlay.Init("248df5355");


        ////// Create the banner object and set the ad unit id 
        ////LevelPlayBannerAd bannerAd = new LevelPlayBannerAd();

        //var configBuilder = new LevelPlayBannerAd.Config.Builder();
        //configBuilder.SetSize(LevelPlayAdSize.LARGE);
        //configBuilder.SetPosition(LevelPlayBannerPosition.TopCenter);
        //configBuilder.SetDisplayOnLoad(true);
        //configBuilder.SetRespectSafeArea(true); // Only relevant for Android
        //configBuilder.SetPlacementName("bannerPlacement");
        //configBuilder.SetBidFloor(1.0); // Minimum bid price in USD
        //var bannerConfig = configBuilder.Build();

        //LevelPlayBannerAd bannerAd = new LevelPlayBannerAd("1mby4s8dvao8a33i", bannerConfig);
    }
}

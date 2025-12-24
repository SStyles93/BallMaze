public static class AdConfig
{
    public static string AppKey => GetAppKey();
    public static string BannerAdUnitId => GetBannerAdUnitId();
    public static string InterstitalAdUnitId => GetInterstitialAdUnitId();
    public static string RewardedVideoAdUnitId => GetRewardedVideoAdUnitId();

    static string GetAppKey()
    {
#if UNITY_ANDROID
        return "248df5355";
#elif UNITY_IPHONE
           return "248df5355";
#else
            return "unexpected_platform";
#endif
    }

    static string GetBannerAdUnitId()
    {
#if UNITY_ANDROID
        return "1mby4s8dvao8a33i";
#elif UNITY_IPHONE
            return "iep3rxsyp9na3rw8";
#else
            return "unexpected_platform";
#endif
    }
    static string GetInterstitialAdUnitId()
    {
#if UNITY_ANDROID
        return "69wkifam6m7enhqo";
#elif UNITY_IPHONE
            return "wmgt0712uuux8ju4";
#else
            return "unexpected_platform";
#endif
    }

    static string GetRewardedVideoAdUnitId()
    {
#if UNITY_ANDROID
        return "7kcyu32zwf1mf20t";
#elif UNITY_IPHONE
            return "qwouvdrkuwivay5q";
#else
            return "unexpected_platform";
#endif
    }
}

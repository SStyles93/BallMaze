public static class AdConfig
{
    public static string AppKey => GetAppKey();
    public static string BannerAdUnitId => GetBannerAdUnitId();
    public static string InterstitalAdUnitId => GetInterstitialAdUnitId();
    public static string RewardedHeartsVideoAdUnitId => GetRewardedHeartsVideoAdUnitId();
    public static string RewardedCoinsVideoAdUnitId => GetRewardedCoinsVideoAdUnitId();

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
        return "gp53lv8cji8vau6e";
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

    static string GetRewardedHeartsVideoAdUnitId()
    {
#if UNITY_ANDROID
        return "m5npfdphbe9fxjhj";
#elif UNITY_IPHONE
            return "qwouvdrkuwivay5q";
#else
            return "unexpected_platform";
#endif
    }

    static string GetRewardedCoinsVideoAdUnitId()
    {
#if UNITY_ANDROID
        return "hn5i748o0zt0zkv4";
#elif UNITY_IPHONE
            return "qwouvdrkuwivay5q";
#else
            return "unexpected_platform";
#endif
    }
}

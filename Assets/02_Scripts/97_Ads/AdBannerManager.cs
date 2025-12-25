using UnityEngine;

public class AdBannerManager : MonoBehaviour
{
    private void Awake()
    {
        if(AdsManager.Instance.BannerAd != null)
            AdsManager.Instance.BannerAd.DestroyAd();

        AdsManager.Instance.LoadTopBanner();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AdsManager.Instance.BannerAd.ShowAd();
    }
}

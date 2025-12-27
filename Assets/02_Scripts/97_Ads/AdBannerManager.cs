using UnityEngine;

public class AdBannerManager : MonoBehaviour
{
    [SerializeField] RectTransform bannerRectTransform;
    //[SerializeField] AdsManager.BannerPosition bannerPosition;

    private void Awake()
    {
        AdsManager adsManager = AdsManager.Instance;
        if (adsManager == null) return;
        if (bannerRectTransform == null) bannerRectTransform = GetComponent<RectTransform>();

        if(AdsManager.Instance.BannerAd != null)
            AdsManager.Instance.BannerAd.DestroyAd();

       // AdsManager.Instance.LoadBanner(bannerRectTransform.rect, bannerPosition, 15);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AdsManager.Instance.BannerAd.ShowAd();
    }
}

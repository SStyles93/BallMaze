using UnityEngine;

public class AdAwareCanvasPadding : MonoBehaviour
{
    [Tooltip("Top-level UI container inside this Canvas")]
    [SerializeField] private RectTransform root;

    private void OnEnable()
    {
        AdsManager.OnBannerHeightChanged += OnBannerHeightChanged;
    }

    private void OnDisable()
    {
        AdsManager.OnBannerHeightChanged -= OnBannerHeightChanged;
    }

    private void Start()
    {
        ApplyPadding(AdsManager.CurrentBannerHeightPx);
    }

    private void OnBannerHeightChanged(int bannerHeightPx)
    {
        ApplyPadding(bannerHeightPx);
    }

    private void ApplyPadding(int bannerHeightPx)
    {
        if (root == null)
            return;

        Vector2 offsetMin = root.offsetMin;
        Vector2 offsetMax = root.offsetMax;

        offsetMax.y = -bannerHeightPx;

        root.offsetMin = offsetMin;
        root.offsetMax = offsetMax;
    }
}

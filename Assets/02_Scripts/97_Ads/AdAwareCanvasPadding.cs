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

        float scaleFactor = root.GetComponentInParent<Canvas>().scaleFactor;

        // Get top safe area inset in pixels
        float safeAreaTop = Screen.height - Screen.safeArea.yMax;

        // Combine banner height and safe area
        float totalTopPadding = (bannerHeightPx + safeAreaTop) / scaleFactor;

        Vector2 offsetMin = root.offsetMin;
        Vector2 offsetMax = root.offsetMax;

        offsetMax.y = -totalTopPadding;

        root.offsetMin = offsetMin;
        root.offsetMax = offsetMax;
    }
}

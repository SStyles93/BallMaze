using UnityEngine;

public class AdAwareCamera : MonoBehaviour
{
    private void OnEnable()
    {
        AdsManager.OnBannerHeightChanged += OnBannerHeightChanged;
    }

    private void OnDisable()
    {
        AdsManager.OnBannerHeightChanged -= OnBannerHeightChanged;
    }

    private void OnBannerHeightChanged(int bannerHeightPx)
    {
        ApplyCameraPadding(bannerHeightPx);
    }

    private void ApplyCameraPadding(int bannerHeightPx)
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;

        // Safe area top in pixels
        float safeAreaTopPx = Screen.height - Screen.safeArea.yMax;

        // Total top padding in pixels
        float totalTopPaddingPx = bannerHeightPx + safeAreaTopPx;

        // Convert to normalized viewport space
        float normalizedTopPadding = totalTopPaddingPx / Screen.height;

        Rect rect = cam.rect;

        rect.y = 0f; // keep bottom anchored
        rect.height = 1f - normalizedTopPadding;

        cam.rect = rect;
    }
}

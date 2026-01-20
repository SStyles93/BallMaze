using UnityEngine;
using UnityEngine.UI;

public class AdUIScaler : MonoBehaviour
{
    private Vector3 originalScale;
    private CanvasScaler canvasScaler;

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
        originalScale = transform.localScale;

        Canvas canvas = GetComponentInParent<Canvas>();
        canvasScaler = canvas.GetComponent<CanvasScaler>();

        ApplyScaling(AdsManager.CurrentBannerHeightPx);
    }

    private void OnBannerHeightChanged(int bannerHeightPx)
    {
        ApplyScaling(bannerHeightPx);
    }

    private void ApplyScaling(int bannerHeightPx)
    {
        if (bannerHeightPx <= 0)
        {
            transform.localScale = originalScale;
            return;
        }

        float screenHeightPx = Screen.height;

        float scaleFactor = (screenHeightPx - bannerHeightPx) / screenHeightPx;
        scaleFactor = Mathf.Clamp(scaleFactor, 0.5f, 1f); // safety clamp

        transform.localScale = originalScale * scaleFactor;
    }
}

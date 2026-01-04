using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoadingOverlay : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CanvasGroup starGroup;
    [SerializeField] private float bgFadeInTime = 0.2f;
    [SerializeField] private float starFadeInTime = 0.5f;
    [SerializeField] private float starFadeOutTime = 0.5f;
    [SerializeField] private float bgFadeOutTime = 0.2f;

    public IEnumerator FadeInBlack()
    {
        // Fade In (alpha 1) of the background first then star
        yield return FadeTo(canvasGroup, 1f, bgFadeInTime);
        yield return FadeTo(starGroup, 1f, starFadeInTime);
    }

    public IEnumerator FadeOutBlack()
    {
        // Fade Out (alpha 0) of the start first then background 
        yield return FadeTo(starGroup,0f, starFadeOutTime);
        yield return FadeTo(canvasGroup, 0f, bgFadeOutTime);
    }

    private IEnumerator FadeTo(CanvasGroup group, float targetAlpha, float duration)
    {
        float groupAlpha = group.alpha;
        float elapsed = 0f;
        while (elapsed < (duration))
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            group.alpha = Mathf.Lerp(groupAlpha, targetAlpha, t);
            yield return null;
        }
        group.alpha = targetAlpha;
    }
}

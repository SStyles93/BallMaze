using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public class TutorialUIElement : MonoBehaviour
{
    [Header("Runtime Animation Data")]
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private bool loop;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Coroutine currentAnimation;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // ==============================
    // DATA INJECTION (CALLED BY MANAGER)
    // ==============================

    public void ApplyStepData(TutorialStep step)
    {
        duration = step.duration;
        loop = step.loop;

        rectTransform.anchoredPosition = step.targetPosition;
        rectTransform.localScale = step.targetScale;
    }

    // ==============================
    // UNITYEVENT-SAFE METHODS
    // ==============================

    public void MoveToTargetPosition()
    {
        StartAnimation(MoveRoutine(rectTransform.anchoredPosition));
    }

    public void ScaleToTarget()
    {
        StartAnimation(ScaleRoutine(rectTransform.localScale));
    }

    public void FadeIn() => StartAnimation(FadeRoutine(1f));
    public void FadeOut() => StartAnimation(FadeRoutine(0f));
    public void SquashAndStretch() => StartAnimation(SquashRoutine());

    public void StopAnimation()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
    }

    // ==============================
    // INTERNALS
    // ==============================

    private void StartAnimation(IEnumerator routine)
    {
        StopAnimation();
        currentAnimation = StartCoroutine(loop ? Loop(routine) : routine);
    }

    private IEnumerator Loop(IEnumerator routine)
    {
        while (true)
            yield return StartCoroutine(routine);
    }

    // ==============================
    // ROUTINES
    // ==============================

    private IEnumerator MoveRoutine(Vector2 target)
    {
        Vector2 start = rectTransform.anchoredPosition;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            rectTransform.anchoredPosition = Vector2.Lerp(start, target, Ease(t / duration));
            yield return null;
        }
    }

    private IEnumerator ScaleRoutine(Vector3 target)
    {
        Vector3 start = rectTransform.localScale;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            rectTransform.localScale = Vector3.Lerp(start, target, Ease(t / duration));
            yield return null;
        }
    }

    private IEnumerator FadeRoutine(float target)
    {
        float start = canvasGroup.alpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, Ease(t / duration));
            yield return null;
        }
    }

    private IEnumerator SquashRoutine()
    {
        yield return ScaleRoutine(new Vector3(1.2f, 0.8f, 1f));
        yield return ScaleRoutine(Vector3.one);
    }

    private float Ease(float t)
    {
        return t * t * (3f - 2f * t); // SmoothStep
    }
}

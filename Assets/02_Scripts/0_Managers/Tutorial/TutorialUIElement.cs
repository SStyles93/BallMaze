using System;
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
    public void MoveToTargetPosition() =>
        StartAnimation(() => MoveRoutine(rectTransform.anchoredPosition));

    public void ScaleToTarget() =>
        StartAnimation(() => ScaleRoutine(rectTransform.localScale));

    public void ScaleUpDown() =>
        StartAnimation(() => ScaleUpDown(0.2f));

    public void FadeIn() =>
        StartAnimation(() => FadeRoutine(1f));

    public void FadeOut() =>
        StartAnimation(() => FadeRoutine(0f));

    public void SquashAndStretch() =>
        StartAnimation(() => SquashRoutine());

    public void StopAnimation()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
    }

    // ==============================
    // INTERNALS
    // ==============================
    private void StartAnimation(Func<IEnumerator> routineFactory)
    {
        StopAnimation();
        currentAnimation = StartCoroutine(loop ? Loop(routineFactory) : routineFactory());
    }

    private IEnumerator Loop(Func<IEnumerator> routineFactory)
    {
        while (true)
            yield return StartCoroutine(routineFactory());
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

        rectTransform.anchoredPosition = target;
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

        rectTransform.localScale = target;
    }

    private IEnumerator ScaleUpDown(float magnitude = 0.2f)
    {
        Vector3 start = rectTransform.localScale;
        float elapsed = 0f;

        while (true) // continuous loop
        {
            elapsed += Time.deltaTime;

            // sine oscillates between -1 and 1
            float sine = Mathf.Sin(elapsed / duration * Mathf.PI * 2f);

            // scale up/down around start
            rectTransform.localScale = start * (1f + sine * magnitude);

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

        canvasGroup.alpha = target;
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

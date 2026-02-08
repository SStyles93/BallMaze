using System;
using System.Collections;
using UnityEngine;

public enum TutorialUIAnimType
{
    MoveToTarget,
    ScaleToTarget,
    FadeIn,
    FadeOut,
    ScaleUpDown,
    SquashAndStretch
}

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public class TutorialUIAnimation : MonoBehaviour
{
    [Header("Defaults")]
    [SerializeField] private Vector3 baseScale = Vector3.one;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Coroutine currentAnimation;

    private Vector2 basePosition;
    private Vector3 initialScale;
    private float initialAlpha;

    private TutorialContext context;
    private string anchorId;
    private float duration;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        initialScale = baseScale;
        initialAlpha = canvasGroup.alpha;
    }

    // ==============================
    // INITIALIZATION (called by manager)
    // ==============================

    public void Initialize(
        TutorialContext context,
        string anchorId,
        float duration
    )
    {
        this.context = context;
        this.anchorId = anchorId;
        this.duration = duration;

        ResolveAnchorPosition();
        CacheBaseState();
    }

    private void ResolveAnchorPosition()
    {
        if (context == null || string.IsNullOrEmpty(anchorId))
            return;

        RectTransform anchor = context.Get(anchorId);
        if (anchor == null)
            return;

        RectTransform parent = rectTransform.parent as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            RectTransformUtility.WorldToScreenPoint(null, anchor.position),
            null,
            out var localPos
        );

        rectTransform.anchoredPosition = localPos;
    }

    private void CacheBaseState()
    {
        basePosition = rectTransform.anchoredPosition;
        rectTransform.localScale = initialScale;
        canvasGroup.alpha = initialAlpha;
    }

    // ==============================
    // ANIMATION ENTRY POINT
    // ==============================

    public void PlaySequence(TutorialUIAnimType[] sequence, bool loop)
    {
        StopAnimation();

        if (sequence == null || sequence.Length == 0)
            return;

        currentAnimation = StartCoroutine(
            loop ? LoopSequence(sequence) : PlaySequenceOnce(sequence)
        );
    }

    public void StopAnimation()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        ResetToBaseState();
    }

    private void ResetToBaseState()
    {
        rectTransform.anchoredPosition = basePosition;
        rectTransform.localScale = initialScale;
        canvasGroup.alpha = initialAlpha;
    }

    // ==============================
    // ANIMATION ROUTINES
    // ==============================

    private IEnumerator PlaySequenceOnce(TutorialUIAnimType[] sequence)
    {
        foreach (var anim in sequence)
            yield return PlaySingle(anim);
    }

    private IEnumerator LoopSequence(TutorialUIAnimType[] sequence)
    {
        while (true)
        {
            foreach (var anim in sequence)
                yield return PlaySingle(anim);

            ResetToBaseState();
        }
    }

    private IEnumerator PlaySingle(TutorialUIAnimType type)
    {
        switch (type)
        {
            case TutorialUIAnimType.FadeIn:
                yield return FadeRoutine(1f);
                break;

            case TutorialUIAnimType.FadeOut:
                yield return FadeRoutine(0f);
                break;

            case TutorialUIAnimType.ScaleUpDown:
                yield return ScaleUpDown(0.2f);
                break;

            case TutorialUIAnimType.MoveToTarget:
                yield return MoveToTargetRoutine();
                break;
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

    private IEnumerator ScaleUpDown(float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float sine = Mathf.Sin(elapsed / duration * Mathf.PI * 2f);
            rectTransform.localScale = initialScale * (1f + sine * magnitude);
            yield return null;
        }

        rectTransform.localScale = initialScale;
    }

    private IEnumerator MoveToTargetRoutine()
    {
        if (context == null || string.IsNullOrEmpty(anchorId))
            yield break;

        // Get the anchor RectTransform
        RectTransform anchor = context.Get(anchorId);
        if (anchor == null)
            yield break;

        // Access start and end positions from TutorialContext
        Vector2 start = Vector2.zero;
        Vector2 end = Vector2.zero;

        // Find the UIAnchor struct for this id
        foreach (var a in context.anchors)
        {
            if (a.id == anchorId)
            {
                start = a.startPosition;
                end = a.endPosition;
                break;
            }
        }

        // Transform local anchor positions to this RectTransform's parent space
        RectTransform parent = rectTransform.parent as RectTransform;
        Vector2 startPos = RectTransformUtility.WorldToScreenPoint(null, anchor.TransformPoint(start));
        Vector2 endPos = RectTransformUtility.WorldToScreenPoint(null, anchor.TransformPoint(end));

        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, startPos, null, out startPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, endPos, null, out endPos);

        // Animate
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Ease(elapsed / duration);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        rectTransform.anchoredPosition = endPos;
    }


    private float Ease(float t)
    {
        return t * t * (3f - 2f * t);
    }
}

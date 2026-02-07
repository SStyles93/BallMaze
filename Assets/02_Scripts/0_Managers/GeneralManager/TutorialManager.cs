using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class Tutorial
{
    [Header("Tutorial Info")]
    public string tutorialName;         // Name or ID
    public int levelNumber;             // Optional metadata
    public string tutorialType;         // e.g., "Movement", "Combat"
    [TextArea(3, 10)]
    public string description;          // Optional description

    [Header("Tutorial Steps")]
    public List<TutorialStep> steps = new List<TutorialStep>();
}

[System.Serializable]
public class TutorialStep
{
    public string description;

    public TutorialUIElement[] tutorialUIElements = new TutorialUIElement[0];

    [Header("Mask / Scrim")]
    public TutorialMaskData maskData;

    [Header("Completion")]
    public bool autoComplete;
    public float waitTime;

    [SerializeReference]
    public ITutorialCondition completionCondition;

    public UnityEvent onStepStart;
    public UnityEvent onStepComplete;
}


[System.Serializable]
public class TutorialUIElement
{
    public TutorialUIAnimation elementAnimation;
    public TutorialUIAnimType[] animationSequence;
    
    [Header("Runtime Animation Data")]
    public float duration = 0.5f;
    public bool loop;

    public string anchorID;
}

[System.Serializable]
public class TutorialMaskData
{
    public bool enableMask = true;

    public string focusTargetId;
    public float padding = 10f;
    public bool blockOutsideClicks = true;
}

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas tutorialCanvas;
    [SerializeField] private CanvasGroup tutorialCanvasGroup;
    [SerializeField] private TutorialOverlayMask overlayMask;

    [Header("Settings")]
    [SerializeField] private float bgFadeInOutTime = 0.1f;
    [Space(10)]
    [SerializeField] private List<Tutorial> tutorialList;

    private TutorialContext currentContext;
    private int currentStepIndex = 0;

    public static TutorialManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ==============================
    // Public Methods to Start Tutorial
    // ==============================

    public void RegisterContext(TutorialContext context)
    {
        currentContext = context;
    }


    public void StartTutorial(int index)
    {
        var tutorial = GetTutorialByIndex(index);
        StartTutorialInternal(tutorial);
    }

    public void StartTutorial(string tutorialName)
    {
        var tutorial = GetTutorialByName(tutorialName);
        StartTutorialInternal(tutorial);
    }

    public void StartTutorialByLevel(int level)
    {
        var tutorial = GetTutorialByLevel(level);
        StartTutorialInternal(tutorial);
    }

    // Internal helper
    private void StartTutorialInternal(Tutorial tutorial)
    {
        if (tutorial == null)
        {
            Debug.LogWarning("Tutorial not found!");
            return;
        }

        currentStepIndex = 0;
        tutorialCanvasGroup.alpha = 0;
        tutorialCanvasGroup.gameObject.SetActive(true);

        StartCoroutine(RunTutorial(tutorial));
    }

    // ==============================
    // Find Methods
    // ==============================

    public Tutorial GetTutorialByIndex(int index)
    {
        if (index >= 0 && index < tutorialList.Count)
            return tutorialList[index];

        Debug.LogWarning("Tutorial index out of range!");
        return null;
    }

    public Tutorial GetTutorialByName(string name)
    {
        var tutorial = tutorialList.Find(t => t.tutorialName == name);
        if (tutorial == null)
            Debug.LogWarning($"Tutorial '{name}' not found!");
        return tutorial;
    }

    public Tutorial GetTutorialByLevel(int level)
    {
        var tutorial = tutorialList.Find(t => t.levelNumber == level);
        if (tutorial == null)
            Debug.LogWarning($"Tutorial for level {level} not found!");
        return tutorial;
    }

    // ==============================
    // Core Coroutine
    // ==============================

    private IEnumerator RunTutorial(Tutorial tutorial)
    {
        yield return FadeTo(tutorialCanvasGroup, 1f, bgFadeInOutTime);

        while (currentStepIndex < tutorial.steps.Count)
        {
            var step = tutorial.steps[currentStepIndex];

            // =========================
            // APPLY MASK (ID-BASED)
            // =========================
            if (step.maskData != null && step.maskData.enableMask && currentContext != null)
            {
                RectTransform target =
                    currentContext.Get(step.maskData.focusTargetId);

                if (target != null)
                {
                    overlayMask.gameObject.SetActive(true);
                    overlayMask.FocusOnTarget(
                        tutorialCanvas,
                        step.maskData.padding,
                        target
                    );
                }
                else
                {
                    overlayMask.gameObject.SetActive(false);
                }
            }
            else
            {
                overlayMask.gameObject.SetActive(false);
            }

            // =========================
            // UI ANIMATION SEQUENCES
            // =========================
            foreach (var uiElement in step.tutorialUIElements)
            {
                var anim = uiElement.elementAnimation;
                if (anim == null) continue;

                anim.Initialize(
                    currentContext,
                    uiElement.anchorID,
                    uiElement.duration
                );

                anim.PlaySequence(
                    uiElement.animationSequence,
                    uiElement.loop
                );
            }


            // =========================
            // STEP START
            // =========================
            step.onStepStart?.Invoke();

            // =========================
            // WAIT FOR COMPLETION
            // =========================
            if (step.autoComplete)
            {
                yield return new WaitForSeconds(step.waitTime);
            }
            else if (step.completionCondition != null)
            {
                yield return new WaitUntil(() =>
                    step.completionCondition.IsSatisfied());
            }
            else
            {
                Debug.LogWarning(
                    $"Tutorial step {currentStepIndex} has no completion condition.");
            }

            // =========================
            // STEP COMPLETE
            // =========================
            step.onStepComplete?.Invoke();
            currentStepIndex++;
        }

        overlayMask.gameObject.SetActive(false);
        yield return FadeTo(tutorialCanvasGroup, 0f, bgFadeInOutTime);
        tutorialCanvasGroup.gameObject.SetActive(false);
    }

    private IEnumerator FadeTo(CanvasGroup group, float targetAlpha, float duration)
    {
        float startAlpha = group.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        group.alpha = targetAlpha;
    }
}

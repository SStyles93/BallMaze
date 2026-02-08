using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public ConditionLogic conditionLogic = ConditionLogic.Any;
    [SerializeReference]
    public List<ITutorialCondition> completionConditions = new List<ITutorialCondition>();

    [Header("Movement Restrictions")]
    public AllowedInput allowedInput = AllowedInput.All;
    public AllowedMovement allowedMovement = AllowedMovement.All;

    public UnityEvent onStepStart;
    public UnityEvent onStepComplete;
}

public enum ConditionLogic
{
    Any,
    All
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
    private bool forceStepComplete = false;


    [HideInInspector]
    public bool IsTutorial1Complete = false, 
        IsTutorialShopComplete = false,
        IsTutorialRocketComplete = false,
        IsTutorialUfoComplete = false;

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

    public void CompleteCurrentStep()
    {
        if (currentContext == null)
            return;

        forceStepComplete = true;
    }

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

    public void SetTutorialComplete(int indexOfTutorial)
    {
        _ = indexOfTutorial switch
        {
            1 => IsTutorial1Complete = true,
            2 => IsTutorialShopComplete = true,
            3 => IsTutorialRocketComplete = true,
            4 => IsTutorialUfoComplete = true,
            _ => false
        };
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
                anim.gameObject.SetActive(true);

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
            // INPUT GATE
            // =========================

            InputGate.Allowed = step.allowedInput;
            MovementGate.Allowed = step.allowedMovement;


            // =========================
            // STEP START
            // =========================
            step.onStepStart?.Invoke();

            // =========================
            // WAIT FOR COMPLETION
            // =========================
            forceStepComplete = false;

            if (step.autoComplete)
            {
                float timer = 0f;

                while (timer < step.waitTime && !forceStepComplete)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
            else if (step.completionConditions != null && step.completionConditions.Count > 0)
            {
                // Bind context to conditions that require it
                foreach (var condition in step.completionConditions)
                {
                    if (condition is IContextBoundCondition boundCondition &&
                        condition is ITargetedTutorialCondition targetedCondition &&
                        currentContext != null)
                    {
                        boundCondition.BindContext(
                            currentContext,
                            targetedCondition.AnchorId,
                            tutorialCanvas
                        );
                    }
                }

                yield return new WaitUntil(() =>
                    forceStepComplete || AreConditionsSatisfied(step));
            }
            else
            {
                Debug.LogWarning(
                    $"Tutorial step {currentStepIndex} has no completion conditions.");
            }


            // =========================
            // STEP COMPLETE
            // =========================

            // --- Constraints ---
            InputGate.Allowed = AllowedInput.All;
            MovementGate.Allowed = AllowedMovement.All;
            // --- UI elements ---
            foreach (var uiElement in step.tutorialUIElements)
            {
                var anim = uiElement.elementAnimation;
                anim.gameObject.SetActive(false);
            }
            // --- Step ---
            step.onStepComplete?.Invoke();
            currentStepIndex++;
            forceStepComplete = false;
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

    /// <summary>
    /// Checks for multiple conditions
    /// </summary>
    /// <param name="step"></param>
    /// <returns></returns>
    private bool AreConditionsSatisfied(TutorialStep step)
    {
        if (step.completionConditions == null || step.completionConditions.Count == 0)
            return true;

        if (step.conditionLogic == ConditionLogic.Any)
            return step.completionConditions.Any(c => c.IsSatisfied());

        return step.completionConditions.All(c => c.IsSatisfied());
    }
}

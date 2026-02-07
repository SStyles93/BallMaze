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

    public Vector3 fingerPosition;      // Optional finger position
    public Vector3 highlightPosition;   // Optional highlight position

    [Header("Position Data")]
    public Vector2 targetPosition;
    public Vector3 targetScale = Vector3.one;
    public float duration = 0.5f;
    public bool loop;

    public bool waitForInput = true;     // Wait for click/tap instead of auto timing

    [Header("Completion")]
    public bool autoComplete;
    public float waitTime;

    [SerializeReference]
    public ITutorialCondition completionCondition;

    public UnityEvent onStepStart;       // Optional actions at step start
    public UnityEvent onStepComplete;    // Optional actions at step end
}

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup tutorialCanvasGroup;
    [SerializeField] private TutorialUIElement fingerUI;
    [SerializeField] private TutorialUIElement highlightUI;


    [Header("Settings")]
    [SerializeField] private float bgFadeInOutTime = 0.1f;

    [Header("Tutorials List")]
    [SerializeField] private List<Tutorial> tutorialList;

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

            // Inject data BEFORE events
            fingerUI?.ApplyStepData(step);
            highlightUI?.ApplyStepData(step);

            // Trigger events
            step.onStepStart?.Invoke();

            if (step.autoComplete)
            {
                yield return new WaitForSeconds(step.waitTime);
            }
            else if (step.completionCondition != null)
            {
                yield return new WaitUntil(() => step.completionCondition.IsSatisfied());
            }
            else
            {
                Debug.LogWarning("Step has no completion condition!");
            }


            step.onStepComplete?.Invoke();
            currentStepIndex++;
        }

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

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public enum InfoLevel
{
    Validation,
    Normal,
    Warning,
    Error
}

public class UserInfoMessage : MonoBehaviour
{
    public static UserInfoMessage Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Image levelInfoImage;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float defaultDisplayTime = 1f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        canvasGroup.alpha = 0f;
    }

    // PUBLIC ACCESS METHOD
    public static void Show(string text, InfoLevel level, float displayTime = -1f)
    {
        if (Instance == null) return;

        if (displayTime <= 0f)
            displayTime = Instance.defaultDisplayTime;

        Instance.InternalShow(text, level, displayTime);
    }

    private void InternalShow(string text, InfoLevel level, float displayTime)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        messageText.text = text;
        levelInfoImage.color = GetColor(level);

        currentRoutine = StartCoroutine(ShowRoutine(displayTime));
    }

    private IEnumerator ShowRoutine(float displayTime)
    {
        yield return Fade(0f, 1f);

        yield return new WaitForSeconds(displayTime);

        yield return Fade(1f, 0f);
    }

    private IEnumerator Fade(float start, float end)
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }

        canvasGroup.alpha = end;
    }

    private Color GetColor(InfoLevel level)
    {
        switch (level)
        {
            case InfoLevel.Validation:
                return Color.green;

            case InfoLevel.Warning:
                return Color.yellow;

            case InfoLevel.Error:
                return Color.red;

            default:
                return Color.white;
        }
    }
}

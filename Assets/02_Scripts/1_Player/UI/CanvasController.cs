using UnityEngine;
using UnityEngine.InputSystem;

public class CanvasController : MonoBehaviour
{
    string currentControlScheme = string.Empty;
    [SerializeField] private CanvasGroup canvasGroup = null;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        PlayerControler.OnControlsChanged += AssignControlScheme;
    }

    private void OnDisable()
    {
        PlayerControler.OnControlsChanged -= AssignControlScheme;
    }

    private void AssignControlScheme(string controlScheme)
    {
        currentControlScheme = controlScheme;
        //if (currentControlScheme == "Keyboard&Mouse") canvasGroup.alpha = 0.0f;
        //else canvasGroup.alpha = 1.0f;
    }
}

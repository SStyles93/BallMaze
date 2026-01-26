using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    string currentControlScheme = string.Empty;
    [SerializeField] private GameObject mobileUI;
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
        if (currentControlScheme == "Keyboard&Mouse") mobileUI.SetActive(false);
        else mobileUI.SetActive(true);
    }
}

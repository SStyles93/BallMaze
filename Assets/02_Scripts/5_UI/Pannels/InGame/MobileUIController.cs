using UnityEngine;

public class MobileUIController : MonoBehaviour
{
    [SerializeField] private string currentControlScheme;

    private void OnEnable()
    {
        PlayerController.OnControlsChanged += UpdateControlVisuals;
    }

    private void OnDisable()
    {
        PlayerController.OnControlsChanged += UpdateControlVisuals;
    }

    private void UpdateControlVisuals(string controlScheme)
    {
        currentControlScheme = controlScheme;

        if (controlScheme != "Touch")
        {
            GetComponentInChildren<Transform>().gameObject.SetActive(false);
        }
    }
}

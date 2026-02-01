using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    string currentControlScheme = string.Empty;
    [SerializeField] private GameObject mobileUI;
    [SerializeField] private GameObject joystickBackground;
    [SerializeField] private GameObject joystick;
    [SerializeField] private float maxJoystickRadius = 150.0f;

    private bool joystickVisible;

    private void OnEnable()
    {
        PlayerController.OnTouchStarted += OnTouchStarted;
        PlayerController.OnMovePerformed += OnMovePerformed;
        PlayerController.OnTouchStopped += OnTouchStopped;
        PlayerController.OnControlsChanged += AssignControlScheme;
    }

    private void OnDisable()
    {
        PlayerController.OnTouchStarted -= OnTouchStarted;
        PlayerController.OnMovePerformed -= OnMovePerformed;
        PlayerController.OnTouchStopped -= OnTouchStopped;
        PlayerController.OnControlsChanged -= AssignControlScheme;
    }

    private void OnTouchStarted(Vector3 screenPosition)
    {
        joystickBackground.transform.position = screenPosition;
        joystick.transform.position = screenPosition;

        joystickVisible = false;
        joystickBackground.SetActive(false);
        joystick.SetActive(false);
    }

    private void OnMovePerformed(Vector2 input)
    {
        if (!joystickVisible && input.magnitude > 0f)
        {
            joystickBackground.SetActive(true);
            joystick.SetActive(true);
            joystickVisible = true;
        }

        Vector2 offset = input * maxJoystickRadius;

        joystick.transform.position =
            joystickBackground.transform.position + (Vector3)offset;
    }

    private void OnTouchStopped()
    {
        joystickBackground.SetActive(false);
        joystick.SetActive(false);
        joystickVisible = false;
    }



    private void AssignControlScheme(string controlScheme)
    {
        currentControlScheme = controlScheme;
        if (currentControlScheme == "Keyboard&Mouse") mobileUI.SetActive(false);
        else mobileUI.SetActive(true);
    }
}

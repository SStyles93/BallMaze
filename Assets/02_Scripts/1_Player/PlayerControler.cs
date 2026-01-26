using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;


[RequireComponent(typeof(PlayerInput))]
public class PlayerControler : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    private Vector2 movementDirection = Vector2.zero;

    #region Touch
    [SerializeField] private float joystickDeadZone = 20f; // pixels
    [SerializeField] private float tapMaxDuration = 0.25f;

    private Finger joystickFinger;
    private Vector2 joystickStartPos;
    private bool joystickFingerDragged;


    /// <summary>
    /// Delegate used to transmit START touch position to other scripts
    /// </summary>
    public static event Action<Vector3> OnTouchStarted = delegate { };

    /// <summary>
    /// Delegate used to transmit CURRENT Touch Position
    /// </summary>
    public static event Action<Vector3> OnTouchPerformed = delegate { };

    /// <summary>
    /// Delegate used to transmit END of Touch
    /// </summary>
    public static event Action OnTouchStopped = delegate { };
    #endregion


    /// <summary>
    /// Delegate used to Transmit movement
    /// </summary>
    public static event Action<Vector2> OnMovePerfromed;

    /// <summary>
    /// Delegate used to transmit Jump action
    /// </summary>
    public static event Action OnJumpPerformed;

    /// <summary>
    /// Delegate used to transmit control changes
    /// </summary>
    public static event Action<string> OnControlsChanged;


    private void Awake()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
        {
            switch (touch.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    OnFingerDown(touch.finger);
                    break;

                case UnityEngine.InputSystem.TouchPhase.Moved:
                    OnFingerMove(touch.finger);
                    break;

                case UnityEngine.InputSystem.TouchPhase.Ended:
                case UnityEngine.InputSystem.TouchPhase.Canceled:
                    OnFingerUp(touch.finger);
                    break;
            }
        }
    }

    public void ControlsChanged(PlayerInput input)
    {
        OnControlsChanged?.Invoke(input.currentControlScheme);
    }


    #region Gamepad & Keyboard

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (GameStateManager.Instance?.CurrentGameState != GameState.Playing)
            return;

        if (ctx.performed)
        {
            movementDirection = ctx.ReadValue<Vector2>();
            // Calls the event for the PlayerMovement
            OnMovePerfromed?.Invoke(movementDirection);
            //Debug.Log(ctx.ReadValue<Vector2>());
        }
        if (ctx.canceled)
        {
            OnMovePerfromed?.Invoke(Vector3.zero);
        }
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (GameStateManager.Instance?.CurrentGameState != GameState.Playing)
            return;

        if (ctx.performed)
        {
            OnJumpPerformed?.Invoke();
            //Debug.Log(ctx.ReadValue<float>());
        }
    }

    #endregion

    #region Touch

    private void OnFingerDown(Finger finger)
    {
        Vector2 pos = finger.screenPosition;

        if (joystickFinger == null)
        {
            joystickFinger = finger;
            joystickStartPos = pos;
            joystickFingerDragged = false;

            OnTouchStarted?.Invoke(pos);
        }
    }

    private void OnFingerMove(Finger finger)
    {
        if (finger != joystickFinger)
            return;

        Vector2 delta = finger.screenPosition - joystickStartPos;

        if (!joystickFingerDragged && delta.magnitude > joystickDeadZone)
        {
            joystickFingerDragged = true;
        }

        if (!joystickFingerDragged)
            return;

        Vector2 normalized = Vector2.ClampMagnitude(delta / joystickDeadZone, 1f);
        OnMovePerfromed?.Invoke(normalized);
    }

    private void OnFingerUp(Finger finger)
    {
        float duration = (float)(
            finger.currentTouch.time - finger.currentTouch.startTime
        );

        float distance = Vector2.Distance(
            finger.currentTouch.startScreenPosition,
            finger.screenPosition
        );

        // Joystick finger released
        if (finger == joystickFinger)
        {
            joystickFinger = null;
            OnTouchStopped?.Invoke();
            OnMovePerfromed?.Invoke(Vector2.zero);

            // TAP → jump
            if (!joystickFingerDragged &&
                duration <= tapMaxDuration &&
                distance <= joystickDeadZone)
            {
                OnJumpPerformed?.Invoke();
            }

            return;
        }

        // Any other finger → jump
        if (duration <= tapMaxDuration && distance <= joystickDeadZone)
        {
            if (!IsPointerOverUI(finger.screenPosition))
                OnJumpPerformed?.Invoke();
        }
    }
    #endregion

    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

}
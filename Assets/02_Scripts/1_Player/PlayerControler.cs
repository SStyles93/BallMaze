using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;


[RequireComponent(typeof(PlayerInput))]
public class PlayerControler : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    #region Touch
    [SerializeField] private float joystickDeadZone = 20f; // pixels
    [SerializeField] private float tapMaxDuration = 0.25f;
    [SerializeField] private float tapWindowTime = 1.0f; // seconds

    private Vector2 movementDirection = Vector2.zero;
    private Vector2 lastMovementDirection = Vector2.zero;
    private float lastMovementTimer = 0f;

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
        if (lastMovementTimer > 0f)
            lastMovementTimer -= Time.deltaTime;

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

        if (joystickFinger != null && !joystickFinger.isActive)
        {
            ResetJoystick();
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
        if (IsPointerOverUI()) return;

        Vector2 pos = finger.screenPosition;

        if (joystickFinger == null)
        {
            joystickFinger = finger;
            joystickStartPos = pos;
            joystickFingerDragged = false;
            OnTouchStarted?.Invoke(pos);
            return;
        }

        // Any other finger → jump immediately
        PerformJump();
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
        SendMovement(normalized);
    }

    private void OnFingerUp(Finger finger)
    {
        if (IsPointerOverUI()) return;

        // Only joystick finger can jump on release
        if (finger != joystickFinger)
            return;

        float duration =
            (float)(finger.currentTouch.time - finger.currentTouch.startTime);

        float distance = Vector2.Distance(
            finger.currentTouch.startScreenPosition,
            finger.screenPosition
        );

        bool isTapJump =
            !joystickFingerDragged &&
            duration <= tapMaxDuration &&
            distance <= joystickDeadZone;

        if (isTapJump)
            PerformJump();

        ResetJoystick();
    }


    // --- PRIVATE METHODS ---

    private void SendMovement(Vector2 input)
    {
        if (input.magnitude > 0.2f)
        {
            lastMovementDirection = input;
            lastMovementTimer = tapWindowTime;
        }

        OnMovePerfromed?.Invoke(input);
    }

    private void PerformJump()
    {
        if (lastMovementTimer > 0f)
        {
            // Re-emit last movement so jump uses it
            OnMovePerfromed?.Invoke(lastMovementDirection);
        }

        OnJumpPerformed?.Invoke();
    }

    private void ResetJoystick()
    {
        joystickFinger = null;
        joystickFingerDragged = false;
        SendMovement(Vector2.zero);
        OnTouchStopped?.Invoke();
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
    #endregion
}
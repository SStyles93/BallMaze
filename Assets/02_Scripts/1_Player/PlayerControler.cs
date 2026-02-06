using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Windows;


[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    #region Touch
    [SerializeField] private float joystickDeadZone = 20f; // pixels
    [SerializeField] private float tapMaxDistance = 35f;   // pixels
    [SerializeField] private float tapWindowTime = 1.0f; // seconds
    [SerializeField] private float tapMaxDuration = 0.25f;

    private Vector2 movementDirection = Vector2.zero;
    private Vector2 lastMovementDirection = Vector2.zero;
    private float lastMovementTimer = 0f;

    private Finger joystickFinger;
    private Vector2 joystickStartPos;
    private double joystickStartTime;
    private bool joystickFingerDragged;

    //[SerializeField] private TMP_Text MOVE_TEXT;
    //[SerializeField] private TMP_Text JUMP_TEXT;

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
    public static event Action<Vector2> OnMovePerformed;

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
            OnMovePerformed?.Invoke(movementDirection);
            //Debug.Log(ctx.ReadValue<Vector2>());
        }
        if (ctx.canceled)
        {
            OnMovePerformed?.Invoke(Vector3.zero);
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
        if (IsPointerOverUI(finger)) return;

        Vector2 pos = finger.screenPosition;

        if (joystickFinger == null)
        {
            joystickFinger = finger;
            joystickStartPos = pos;
            joystickStartTime = Time.timeAsDouble;
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
        if (IsPointerOverUI(finger)) return;

        // Only joystick finger can jump on release
        if (finger != joystickFinger)
            return;

        float duration = (float)(Time.timeAsDouble - joystickStartTime);
        float distance = Vector2.Distance(joystickStartPos, finger.screenPosition);

        bool isTapJump =
            !joystickFingerDragged &&
            duration <= tapMaxDuration &&
            distance <= tapMaxDistance;

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

        OnMovePerformed?.Invoke(input);
        //MOVE_TEXT.text = $"Move - {input}";
    }

    private void PerformJump()
    {
        if (lastMovementTimer > 0f)
        {
            // Re-emit last movement so jump uses it
            OnMovePerformed?.Invoke(lastMovementDirection);
        }

        OnJumpPerformed?.Invoke();
        //JUMP_TEXT.text = $"Jump with {lastMovementDirection}";
    }

    private void ResetJoystick()
    {
        joystickFinger = null;
        joystickFingerDragged = false;
        SendMovement(Vector2.zero);
        OnTouchStopped?.Invoke();
    }

    /// <summary>
    /// Returns true if the given finger is over UI, blocking gameplay input.
    /// Works for touch and mouse.
    /// </summary>
    private bool IsPointerOverUI(Finger finger)
    {
        if (EventSystem.current == null) return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = finger.screenPosition
        };

        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);
        return raycastResults.Count > 0;
    }
    #endregion
}
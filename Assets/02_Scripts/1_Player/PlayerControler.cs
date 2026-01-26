using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerControler : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    private Vector2 movementDirection = Vector2.zero;


    #region Touch
    [SerializeField] private float tapMaxDistance = 0.5f;   // world units
    [SerializeField] private float tapMaxDuration = 0.2f;   // seconds

    private Vector3 m_startPosition = new Vector3();
    private Vector3 m_currentPosition = new Vector3();

    private bool isFirstTouch = true;
    public static bool isTouchUsed = false;
    private bool isDragging;
    private float touchStartTime;


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

    public void TouchPosition(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        Vector2 screenPosition = ctx.ReadValue<Vector2>();
        Vector3 touchPosition = Camera.main.ScreenToWorldPoint(new Vector3
                (screenPosition.x, screenPosition.y, -Camera.main.transform.position.z));

        if (isFirstTouch)
        {
            //*********************************************************************
            //Updates the state of action and gets the start position of touch
            //*********************************************************************

            m_startPosition = touchPosition;
            m_currentPosition = touchPosition;

            touchStartTime = Time.time;
            isDragging = false;

            isFirstTouch = false;

            //*********************************************************************

            OnTouchStarted?.Invoke(m_startPosition);
            return;
        }

        //*********************************************************************
        //Updates the state of action and gets the current position of touch
        //*********************************************************************

        m_currentPosition = touchPosition;

        float distance = Vector3.Distance(m_currentPosition, m_startPosition);

        // Only switch to PERFORMED if player actually drags
        if (!isDragging && distance > tapMaxDistance)
        {
            isDragging = true;
        }

        if (isDragging)
        {
            movementDirection =  m_currentPosition - m_startPosition;

            OnMovePerfromed?.Invoke(movementDirection);
        }

        //*********************************************************************
    }

    public void TouchPress(InputAction.CallbackContext ctx)
    {
        if (!ctx.canceled) return;

        float touchDuration = Time.time - touchStartTime;
        float distance = Vector3.Distance(m_currentPosition, m_startPosition);


        //*********************************************************************
        //Updates the state of action and gets the last position of touch
        //*********************************************************************
        if (!isDragging && touchDuration <= tapMaxDuration && distance <= tapMaxDistance)
        {
            // Jump
            OnJumpPerformed?.Invoke();
        }
        else
        {
            OnTouchStopped?.Invoke();
        }

        // -- Reset --
        isFirstTouch = true;
        isDragging = false;
        //*********************************************************************
        // Other Updates depending of controls have to be under this line
    }


    #endregion
}
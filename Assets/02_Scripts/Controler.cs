using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(PlayerInput))]
public class Controler : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    private Vector3 m_startPosition = new Vector3();
    private Vector3 m_currentPosition = new Vector3();
    private bool isFirstTouch = true;

    public static bool isTouchUsed = false;
    public static Vector3 deltaPosition = Vector3.zero;

    /// <summary>
    /// Delegate used to transmit START touch position to other scripts
    /// </summary>
    public static event Action<Vector3> TouchStarted = delegate { };

    /// <summary>
    /// Delegate used to transmit CURRENT Touch Position
    /// </summary>
    public static event Action<Vector3> TouchPerformed = delegate { };

    /// <summary>
    /// Delegate used to transmit END of Touch
    /// </summary>
    public static event Action TouchStopped = delegate { };

    private void Awake()
    {
        if(playerInput == null)
        playerInput = GetComponent<PlayerInput>();
    }

    public void TouchPosition(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Vector2 screenPosition = ctx.ReadValue<Vector2>();
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(new Vector3
                    (screenPosition.x, screenPosition.y, -Camera.main.transform.position.z));
            if (isFirstTouch)
            {
                //*********************************************************************
                //Updates the state of action and gets the start position of touch
                //*********************************************************************
                m_startPosition = touchPosition;
                isFirstTouch = false;
                //m_currentActionState = ActionState.STARTED;
                //*********************************************************************
                // Other Updates depending of controls have to be under this line 

                //objectManager.UpdateControls();

                TouchStarted(m_startPosition);
                Debug.Log("Started");
                isTouchUsed = true;
            }
            //*********************************************************************
            //Updates the state of action and gets the current position of touch
            //*********************************************************************
            m_currentPosition = touchPosition;
            
            //Send Message
            TouchPerformed(m_currentPosition);
            Debug.Log("Started");

            //Set Static variables
            deltaPosition = m_currentPosition - m_startPosition;
            isTouchUsed = true;
        }

    }
    public void TouchPress(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled)
        {
            //*********************************************************************
            // Reset the value of isTouched 
            //*********************************************************************
            isFirstTouch = true;
            // Send message
            TouchStopped();
            Debug.Log("Stopped");

            //Reset static variables
            deltaPosition = Vector3.zero;
            isTouchUsed = false;
        }
    }
}

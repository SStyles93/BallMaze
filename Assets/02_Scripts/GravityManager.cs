using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


public class GravityManager : MonoBehaviour
{
    [Header("Unity Components")]
    [Header("Screen")]
    [SerializeField] private Vector2 m_gravityLimit = new Vector2(25, 25);
    [SerializeField] private float m_gyroSensibility = 60.0f;

    [SerializeField] private bool EnableDebugTools = false;
    [ConditionalField("EnableDebugTools")][SerializeField] private TMP_Text m_gravityText;
    [ConditionalField("EnableDebugTools")][SerializeField] private TMP_Text m_gyroText;


    //Private components
    private UnityEngine.Gyroscope m_gyro;
    private Vector3 m_gravity = new Vector3(0, -9.81f, 0);
    private Vector3 m_currentGyroGravity = Vector3.zero;

    /// <summary>
    /// Delegate used to send grativy information
    /// </summary>
    public static event Action<Vector3> GravityChanged = delegate { };

    private void Awake()
    {
        m_gravityText.gameObject.SetActive(EnableDebugTools);
        m_gyroText.gameObject.SetActive(EnableDebugTools);

    }

    void Start()
    {
        //Set up and enable the gyroscope (check if device has one)
        m_gyro = Input.gyro;
        m_gyro.enabled = true;
        m_currentGyroGravity = m_gyro.gravity;
    }

    void Update()
    {
        UpdateGravity();

        if (Controler.isTouchUsed)
        {
            SetGyroGravity();
        }

        m_gravityText.gameObject.SetActive(EnableDebugTools);
        m_gyroText.gameObject.SetActive(EnableDebugTools);
        if (EnableDebugTools)
        {
            UpdateGravityText();
            UpdateGyroText();
        }
    }

    private void LimitGravity()
    {
        //Limit the pivots rotation X at (-17 / 17);        
        m_gravity.x = m_gravity.x > m_gravityLimit.x ? m_gravityLimit.x : m_gravity.x;
        m_gravity.x = m_gravity.x < -m_gravityLimit.x ? -m_gravityLimit.x : m_gravity.x;
        //Limit the pivots rotation  Z at (-17 / 17);        
        m_gravity.z = m_gravity.z > m_gravityLimit.y ? m_gravityLimit.y : m_gravity.z;
        m_gravity.z = m_gravity.z < -m_gravityLimit.y ? -m_gravityLimit.y : m_gravity.z;
    }

    /// <summary>
    /// Resets the Gyro base gravity 
    /// </summary>
    private void SetGyroGravity()
    {
        m_currentGyroGravity = m_gyro.gravity;
    }

    /// <summary>
    /// Update the gravity acording to the Gyroscope
    /// </summary>
    private void UpdateGravity()
    {
        if (Controler.isTouchUsed)
        {
            m_gravity = Controler.deltaPosition;
        }
        else
        {
            m_gravity = new Vector3(
                m_gyro.gravity.x - m_currentGyroGravity.x, 0, m_gyro.gravity.y - m_currentGyroGravity.y)
                * m_gyroSensibility;
        }

        //Set Y gravity to Physics gravity Y (-9.81f)
        m_gravity.y = Physics.gravity.y;
        LimitGravity();

        //Update Physics gravity
        Physics.gravity = m_gravity;
        //Send Message with gravity value
        GravityChanged(m_gravity);
    }

    /// <summary>
    /// Updates the TMP text component according to Gyroscope gravity
    /// </summary>
    private void UpdateGravityText()
    {
        //Set Gravity Text
        m_gravityText.text =
            "Gravity:"
            + $"\n X: {m_gravity.x.ToString()}"
            + $"\n Y: {m_gravity.y.ToString()}"
            + $"\n Z: {m_gravity.z.ToString()}";
    }

    private void UpdateGyroText()
    {
        m_gyroText.text =
            $"Gyro: {m_gyro.ToString()}"
            + $"\nGyro grav {m_gyro.gravity}"
            + $"\nCurrent Gyro Grav:\n{m_currentGyroGravity}";
    }
}
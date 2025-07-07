using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MyBox;

[RequireComponent(typeof(Controler))]
public class ObjectManager : MonoBehaviour
{
    [SerializeField] AnimationCurve forceCurve;
    [Tooltip("Enables the Visuals tools for force debug")]
    [SerializeField] bool EnableDebugTools = false;
    [ConditionalField("EnableDebugTools")] [SerializeField] Gradient forceGradient;
    [ConditionalField("EnableDebugTools")] [SerializeField] int gradientResolution = 4;

    [SerializeField] Vector3 m_beginPos;
    [SerializeField] Vector3 m_endPos;

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    private void Start()
    {
       
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (!EnableDebugTools) return;
        for (int i = 1; i < gradientResolution; i++)
        {
            float gradientPrecision = 1.0f / gradientResolution;
            Gizmos.color = forceGradient.Evaluate(forceCurve.Evaluate(i * gradientPrecision));
            Gizmos.DrawLine(m_beginPos, m_endPos);
        }
    }

    ///// <summary>
    ///// Updates the Objects according to action state
    ///// </summary>
    //public void UpdateControls()
    //{
    //    switch (controler.CurrentActionState)
    //    {
    //        case ActionState.STARTED:
    //            Debug.Log("STARTED");
    //            Vector3 tmpStartPos = controler.StartPosition;
    //            break;

    //        case ActionState.PERFORMED:
    //            Debug.Log("PERFORMED");

    //            break;

    //        case ActionState.STOPPED:
    //            Debug.Log("STOPPED");
    //            //Get delta from m_currentPos - m_startPos to apply to all objects according to distance from force source
    //            float deltaPosition = (controler.CurrentPosition - controler.StartPosition).magnitude;
    //            break;
    //    }
    //}
}

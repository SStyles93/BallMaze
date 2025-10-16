using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineDrawer : MonoBehaviour
{
    //Private variables
    private LineRenderer line;
    [SerializeField] float colorLerpRange = 50.0f;

    private void OnEnable()
    {
        //Controler.TouchStarted += SetLineStartPosition;
        //Controler.TouchPerformed += SetLineEndPosition;
        //Controler.TouchStopped += DisableLine;
    }

    private void OnDisable()
    {
        //Controler.TouchStarted -= SetLineStartPosition;
        //Controler.TouchPerformed -= SetLineEndPosition;
        //Controler.TouchStopped -= DisableLine;
    }

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    private void DisableLine()
    {
        line.enabled = false;
    }

    public void SetLineStartPosition(Vector3 position)
    {
        line.enabled = true;
        line.SetPosition(0, position);
    }

    public void SetLineEndPosition(Vector3 position)
    {
        line.SetPosition(1, position);
        SetColorFromDistance(colorLerpRange);
    }

    /// <summary>
    /// Lerps the Line Renderer's color according to delta(endPos-startPos)/objectMaxDistance
    /// </summary>
    /// <param name="objectMaxDistance">The maximal distance obj objects, used as max lerp parameter</param>
    private void SetColorFromDistance(float objectMaxDistance)
    {
        line.sharedMaterial.color = Color.Lerp(
            line.startColor,
            line.endColor,
            (line.GetPosition(1) - line.GetPosition(0)).magnitude / objectMaxDistance
            );
    }
}

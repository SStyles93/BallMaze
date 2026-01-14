using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// An add-on module for Cinemachine Virtual Camera that locks the camera's Y co-ordinate
/// </summary>
[SaveDuringPlay]
[AddComponentMenu("")] // Hide in menu
public class CameraClamp : CinemachineExtension
{
    [SerializeField] private float softZone;
    [SerializeField] private Vector2 xLimits = Vector2.zero;

    public void SetXLimits(Vector2 limits)
    {
        xLimits = limits;
    }

    public void SetSoftZone(float softZone)
    {
        this.softZone = softZone;
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state, float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Finalize)
            return;

        Vector3 pos = state.RawPosition;

        // Left limit
        if (pos.x < xLimits.x + softZone)
        {
            float t = Mathf.InverseLerp(xLimits.x, xLimits.x + softZone, pos.x);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            pos.x = Mathf.Lerp(xLimits.x, pos.x, eased);
        }

        // Right limit
        if (pos.x > xLimits.y - softZone)
        {
            float t = Mathf.InverseLerp(xLimits.y, xLimits.y - softZone, pos.x);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            pos.x = Mathf.Lerp(xLimits.y, pos.x, eased);
        }

        // Safety clamp
        pos.x = Mathf.Clamp(pos.x, xLimits.x, xLimits.y);

        state.RawPosition = pos;
    }
}


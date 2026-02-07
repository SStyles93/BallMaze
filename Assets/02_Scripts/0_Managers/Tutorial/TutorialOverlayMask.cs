using UnityEngine;

/// <summary>
/// Overlay mask for tutorial that surrounds a target RectTransform
/// without moving the panels (anchors remain fixed). Only scale changes.
/// </summary>
public class TutorialOverlayMask : MonoBehaviour
{
    public RectTransform Top, Bottom, Left, Right;

    /// <summary>
    /// Focus the mask around the given rect, with padding.
    /// </summary>
    public void FocusOnTarget(Canvas canvas, float padding, RectTransform target)
    {
        if (target == null) return;

        RectTransform canvasRect = canvas.transform as RectTransform;

        // --- Canvas world corners ---
        Vector3[] canvasCorners = new Vector3[4];
        canvasRect.GetWorldCorners(canvasCorners);
        Vector3 canvasBL = canvasCorners[0];
        Vector3 canvasTL = canvasCorners[1];
        Vector3 canvasTR = canvasCorners[2];
        Vector3 canvasBR = canvasCorners[3];

        // --- Target world corners ---
        Vector3[] t = new Vector3[4];
        target.GetWorldCorners(t);
        Vector3 targetBL = t[0] - new Vector3(padding, padding, 0);
        Vector3 targetTL = t[1] + new Vector3(-padding, padding, 0);
        Vector3 targetTR = t[2] + new Vector3(padding, -padding, 0);
        Vector3 targetBR = t[3] + new Vector3(-padding, -padding, 0);

        // --- SCALE PANELS WITHOUT MOVING ---
        ScaleTopBottom(Top, canvasTR.y, targetTL.y);
        ScaleTopBottom(Bottom, targetBL.y, canvasBL.y);
        ScaleLeftRight(Left, canvasBL.x, targetTL.x);
        ScaleLeftRight(Right, targetTR.x, canvasTR.x);
    }

    /// <summary>
    /// Scale a top/bottom panel anchored to canvas top/bottom.
    /// Position stays fixed. Height = distance to target edge.
    /// </summary>
    private void ScaleTopBottom(RectTransform rect, float canvasEdgeY, float targetEdgeY)
    {
        if (rect == null) return;

        float newHeight = Mathf.Abs(canvasEdgeY - targetEdgeY);
        Vector2 size = rect.sizeDelta;
        size.y = newHeight;
        rect.sizeDelta = size;
    }

    /// <summary>
    /// Scale a left/right panel anchored to canvas left/right.
    /// Position stays fixed. Width = distance to target edge.
    /// </summary>
    private void ScaleLeftRight(RectTransform rect, float canvasEdgeX, float targetEdgeX)
    {
        if (rect == null) return;

        float newWidth = Mathf.Abs(canvasEdgeX - targetEdgeX);
        Vector2 size = rect.sizeDelta;
        size.x = newWidth;
        rect.sizeDelta = size;
    }
}

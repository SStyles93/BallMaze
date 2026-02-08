using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public class TapAnywhereCondition : ITutorialCondition
{
    public bool IsSatisfied()
    {
        return Pointer.current != null &&
               Pointer.current.press.wasPressedThisFrame;
    }
}


[System.Serializable]
public class TapInAreaCondition : ITutorialCondition, IContextBoundCondition
{
    public float padding = 0f;
    private Rect screenRect;

    public void BindContext(TutorialContext context, string anchorId, Canvas canvas)
    {
        var rect = context?.Get(anchorId);
        if (rect == null) return;

        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        Vector2 min = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

        screenRect = Rect.MinMaxRect(
            min.x - padding,
            min.y - padding,
            max.x + padding,
            max.y + padding
        );
    }

    public bool IsSatisfied()
    {
        var pointer = Pointer.current;
        if (pointer == null || !pointer.press.wasPressedThisFrame)
            return false;

        return screenRect.Contains(pointer.position.ReadValue());
    }
}

[System.Serializable]
public class TapAndReleaseInAreaCondition : ITutorialCondition, IContextBoundCondition
{
    [Header("Tap Tolerance")]
    public float padding = 0f;
    public float maxMoveDistance = 40f;   // pixels
    public float maxDuration = 0.3f;      // seconds

    private Rect screenRect;

    private Vector2 pressPosition;
    private float pressTime;
    private bool pressedInside;

    public void BindContext(
        TutorialContext context,
        string anchorId,
        Canvas canvas)
    {
        var rect = context?.Get(anchorId);
        if (rect == null) return;

        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        Vector2 min = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

        screenRect = Rect.MinMaxRect(
            min.x - padding,
            min.y - padding,
            max.x + padding,
            max.y + padding
        );
    }

    public bool IsSatisfied()
    {
        var pointer = Pointer.current;
        if (pointer == null) return false;

        Vector2 pos = pointer.position.ReadValue();

        // Press
        if (pointer.press.wasPressedThisFrame)
        {
            pressPosition = pos;
            pressTime = Time.time;
            pressedInside = screenRect.Contains(pos);
            return false;
        }

        // Release
        if (pressedInside && pointer.press.wasReleasedThisFrame)
        {
            pressedInside = false;

            float duration = Time.time - pressTime;
            float distance = Vector2.Distance(pressPosition, pos);

            return
                screenRect.Contains(pos) &&
                duration <= maxDuration &&
                distance <= maxMoveDistance;
        }

        return false;
    }
}

[System.Serializable]
public class SwipeCondition : ITutorialCondition
{
    public Vector2 direction;
    public float minDistance = 100f;

    private Vector2 start;
    private bool isSwiping = false;

    public bool IsSatisfied()
    {
        var pointer = Pointer.current;
        if (pointer == null) return false;

        // When the swipe starts
        if (pointer.press.wasPressedThisFrame)
        {
            start = pointer.position.ReadValue();
            isSwiping = true;
        }

        // Check while swiping
        if (isSwiping && pointer.press.isPressed)
        {
            Vector2 delta = pointer.position.ReadValue() - start;
            if (delta.magnitude >= minDistance &&
                Vector2.Dot(delta.normalized, direction.normalized) > 0.8f)
            {
                isSwiping = false; // reset to avoid multiple triggers
                return true;
            }
        }

        // Reset if pointer released before reaching minDistance
        if (pointer.press.wasReleasedThisFrame)
        {
            isSwiping = false;
        }

        return false;
    }
}




[System.Serializable]
public class SwipeAndReleaseCondition : ITutorialCondition
{
    public Vector2 direction;
    public float minDistance = 100f;

    private Vector2 start;

    public bool IsSatisfied()
    {
        var pointer = Pointer.current;
        if (pointer == null) return false;

        if (pointer.press.wasPressedThisFrame)
            start = pointer.position.ReadValue();

        if (pointer.press.wasReleasedThisFrame)
        {
            Vector2 delta = pointer.position.ReadValue() - start;
            return delta.magnitude >= minDistance &&
                   Vector2.Dot(delta.normalized, direction.normalized) > 0.8f;
        }

        return false;
    }

}

[System.Serializable]
public class TapOnUIElementCondition : ITutorialCondition
{
    public Graphic targetGraphic;

    public bool IsSatisfied()
    {
        if (targetGraphic == null || Mouse.current == null)
            return false;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject == targetGraphic.gameObject)
                return true;
        }

        return false;
    }
}


[System.Serializable]
public class DragFromToCondition : ITutorialCondition, IContextBoundCondition
{
    [Min(0)] public float startRadius = 80f;
    [Min(0)] public float endRadius = 80f;
    [Min(0)] public float minDragDistance = 150f;

    private Vector2 startScreen;
    private Vector2 endScreen;

    private Vector2 dragStart;
    private bool dragging;

    public void BindContext(TutorialContext context, string anchorId, Canvas canvas)
    {
        if (context == null) return;

        var uiAnchor = context.anchors.FirstOrDefault(a => a.id == anchorId);
        if (uiAnchor.rect == null) return;

        Vector2 startWorld = uiAnchor.rect.TransformPoint(uiAnchor.startPosition);
        Vector2 endWorld = uiAnchor.rect.TransformPoint(uiAnchor.endPosition);

        startScreen = RectTransformUtility.WorldToScreenPoint(null, startWorld);
        endScreen = RectTransformUtility.WorldToScreenPoint(null, endWorld);
    }

    public bool IsSatisfied()
    {
        var pointer = Pointer.current;
        if (pointer == null) return false;

        Vector2 pos = pointer.position.ReadValue();

        // Start drag if pointer pressed near start
        if (pointer.press.wasPressedThisFrame)
        {
            dragStart = pos;
            dragging = Vector2.Distance(dragStart, startScreen) <= startRadius;
        }

        // Check while dragging (pointer still pressed)
        if (dragging && pointer.press.isPressed)
        {
            Vector2 delta = pos - dragStart;

            float distanceToEnd = Vector2.Distance(pos, endScreen);
            float dragDistance = delta.magnitude;

            if (dragDistance >= minDragDistance && distanceToEnd <= endRadius)
            {
                dragging = false; // reset so it triggers only once
                return true;
            }
        }

        // Reset if pointer released before satisfying
        if (pointer.press.wasReleasedThisFrame)
            dragging = false;

        return false;
    }
}



[System.Serializable]
public class DragFromReleaseAtCondition : ITutorialCondition, IContextBoundCondition
{
    [Min (0)] public float startRadius = 80f;
    [Min(0)] public float endRadius = 80f;
    [Min(0)] public float minDragDistance = 150f;

    private Vector2 startScreen;
    private Vector2 endScreen;

    private Vector2 dragStart;
    private bool dragging;

    public void BindContext(TutorialContext context, string anchorId, Canvas canvas)
    {
        if (context == null) return;

        var uiAnchor = context.anchors.FirstOrDefault(a => a.id == anchorId);
        if (uiAnchor.rect == null) return;

        Vector2 startWorld = uiAnchor.rect.TransformPoint(uiAnchor.startPosition);
        Vector2 endWorld = uiAnchor.rect.TransformPoint(uiAnchor.endPosition);

        startScreen = RectTransformUtility.WorldToScreenPoint(null, startWorld);
        endScreen = RectTransformUtility.WorldToScreenPoint(null, endWorld);
    }

    public bool IsSatisfied()
    {
        var pointer = Pointer.current;
        if (pointer == null) return false;

        Vector2 pos = pointer.position.ReadValue();

        if (pointer.press.wasPressedThisFrame)
        {
            dragStart = pos;
            dragging = Vector2.Distance(dragStart, startScreen) <= startRadius;
        }

        if (dragging && pointer.press.wasReleasedThisFrame)
        {
            Vector2 dragEnd = pos;

            dragging = false;
            return
                Vector2.Distance(dragEnd, endScreen) <= endRadius &&
                Vector2.Distance(dragStart, dragEnd) >= minDragDistance;
        }

        return false;
    }
}



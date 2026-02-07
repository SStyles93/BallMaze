using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[System.Serializable]
public class TapAnywhereCondition : ITutorialCondition
{
    public bool IsSatisfied()
    {
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }
}

[System.Serializable]
public class TapInAreaCondition : ITutorialCondition
{
    public Rect screenArea;

    public bool IsSatisfied()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return false;

        Vector2 pos = Mouse.current.position.ReadValue();
        return screenArea.Contains(pos);
    }
}

[System.Serializable]
public class SwipeCondition : ITutorialCondition
{
    public Vector2 direction;
    public float minDistance = 100f;

    private Vector2 start;

    public bool IsSatisfied()
    {
        if (Mouse.current == null) return false;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            start = Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Vector2 delta = Mouse.current.position.ReadValue() - start;
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
public class DragFromToCondition : ITutorialCondition
{
    public Vector2 startAreaCenter;
    public float startRadius = 80f;

    public Vector2 endAreaCenter;
    public float endRadius = 80f;

    public float minDragDistance = 150f;

    private Vector2 dragStart;
    private bool dragging;

    public bool IsSatisfied()
    {
        if (Mouse.current == null) return false;

        // Start drag
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            dragStart = Mouse.current.position.ReadValue();
            dragging = Vector2.Distance(dragStart, startAreaCenter) <= startRadius;
        }

        // End drag
        if (dragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Vector2 dragEnd = Mouse.current.position.ReadValue();
            bool reachedEnd = Vector2.Distance(dragEnd, endAreaCenter) <= endRadius;
            bool longEnough = Vector2.Distance(dragStart, dragEnd) >= minDragDistance;

            dragging = false;
            return reachedEnd && longEnough;
        }

        return false;
    }
}

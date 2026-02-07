using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class TapAnywhereCondition : ITutorialCondition
{
    public bool IsSatisfied()
    {
        return Input.GetMouseButtonDown(0);
    }
}

[System.Serializable]
public class TapInAreaCondition : ITutorialCondition
{
    public Rect screenArea;

    public bool IsSatisfied()
    {
        if (!Input.GetMouseButtonDown(0))
            return false;

        Vector2 pos = Input.mousePosition;
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
        if (Input.GetMouseButtonDown(0))
            start = Input.mousePosition;

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - start;
            return delta.magnitude >= minDistance &&
                   Vector2.Dot(delta.normalized, direction.normalized) > 0.8f;
        }

        return false;
    }
}

[System.Serializable]
public class TapOnUIElementCondition : ITutorialCondition
{
    [Tooltip("UI element that must be tapped")]
    public Graphic targetGraphic;

    public bool IsSatisfied()
    {
        if (targetGraphic == null)
            return false;

        if (!Input.GetMouseButtonDown(0))
            return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
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
    [Header("Drag Settings")]
    public Vector2 startAreaCenter;
    public float startRadius = 80f;

    public Vector2 endAreaCenter;
    public float endRadius = 80f;

    public float minDragDistance = 150f;

    private Vector2 dragStart;
    private bool dragging;

    public bool IsSatisfied()
    {
        // Start drag
        if (Input.GetMouseButtonDown(0))
        {
            dragStart = Input.mousePosition;
            dragging = Vector2.Distance(dragStart, startAreaCenter) <= startRadius;
        }

        // End drag
        if (dragging && Input.GetMouseButtonUp(0))
        {
            Vector2 dragEnd = Input.mousePosition;

            bool reachedEnd =
                Vector2.Distance(dragEnd, endAreaCenter) <= endRadius;

            bool longEnough =
                Vector2.Distance(dragStart, dragEnd) >= minDragDistance;

            dragging = false;
            return reachedEnd && longEnough;
        }

        return false;
    }
}





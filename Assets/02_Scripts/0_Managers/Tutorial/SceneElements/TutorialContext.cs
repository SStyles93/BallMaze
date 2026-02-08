using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TutorialContext : MonoBehaviour
{
    [System.Serializable]
    public struct UIAnchor
    {
        public string id;
        public RectTransform rect;
        public Vector2 startPosition;
        public Vector2 endPosition;
        public bool gizmos;
    }

    public UIAnchor[] anchors;

    //Check if tutorial was played TutorialManager & SavingSystem

    private void Awake()
    {
        TutorialManager.Instance.RegisterContext(this);
    }

    public RectTransform Get(string id)
    {
        foreach (var a in anchors)
            if (a.id == id)
                return a.rect;

        Debug.LogWarning($"Tutorial anchor '{id}' not found");
        return null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (anchors == null) return;

        foreach (var anchor in anchors)
        {
            if (!anchor.gizmos) return;

            if (anchor.rect == null) continue;

            // Get world corners of the RectTransform
            Vector3[] corners = new Vector3[4];
            anchor.rect.GetWorldCorners(corners);

            // Convert start/end from local rect space (0-1) to world position
            Vector3 startWorld = anchor.rect.TransformPoint(anchor.startPosition);
            Vector3 endWorld = anchor.rect.TransformPoint(anchor.endPosition);

            // Draw handles in Scene view
            Handles.color = Color.green;
            Handles.DrawLine(startWorld, endWorld);
            Handles.DrawSolidDisc(startWorld, Vector3.forward, 5f);
            Handles.DrawSolidDisc(endWorld, Vector3.forward, 5f);

            // Label
            Handles.Label(startWorld + Vector3.up * 10f, $"{anchor.id} Start");
            Handles.Label(endWorld + Vector3.up * 10f, $"{anchor.id} End");
        }
    }
#endif
}

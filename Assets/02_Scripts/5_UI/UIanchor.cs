using UnityEngine;

public class UIanchor : MonoBehaviour
{
    public Transform target;          // 3D object
    public Vector3 worldOffset;        // Offset above the object
    public RectTransform uiElement;    // UI element (Image, Text, etc.)

    Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        Vector3 screenPos = cam.WorldToScreenPoint(target.position + worldOffset);

        // Optional: hide if behind camera
        if (screenPos.z < 0)
        {
            uiElement.gameObject.SetActive(false);
            return;
        }

        uiElement.gameObject.SetActive(true);
        uiElement.position = screenPos;
    }
}

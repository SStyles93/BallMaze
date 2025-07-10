using UnityEditor;
using UnityEngine;

public class TextureZoomWindow : EditorWindow
{
    private Texture2D texture;
    private Vector2 scrollPosition = Vector2.zero;
    private float zoom = 1.0f;
    private const float MinZoom = 0.1f;
    private const float MaxZoom = 10.0f;

    private Rect _textureAreaRect;

    [MenuItem("Window/Texture Zoom")]
    public static void ShowWindow()
    {
        GetWindow<TextureZoomWindow>("Texture Zoom");
    }

    private void OnGUI()
    {
        texture = (Texture2D)EditorGUILayout.ObjectField("Texture", texture, typeof(Texture2D), false);

        if (texture == null)
        {
            EditorGUILayout.HelpBox("Please assign a texture to see the preview.", MessageType.Info);
            return;
        }

        // Reserve a flexible area for the scroll view
        Rect scrollViewRect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        scrollPosition = GUI.BeginScrollView(scrollViewRect, scrollPosition, GetContentRect());

        // Handle zoom and pan events
        HandleEvents();

        // Draw the texture
        if (Event.current.type == EventType.Repaint)
        {
            GUI.DrawTexture(_textureAreaRect, texture, ScaleMode.ScaleToFit);
        }

        GUI.EndScrollView();

        // Display the current zoom level at the bottom
        EditorGUILayout.LabelField("Zoom", $"{zoom:F2}x");
    }

    private Rect GetContentRect()
    {
        // Calculate the size of the content based on the texture and zoom
        float zoomedWidth = texture.width * zoom;
        float zoomedHeight = texture.height * zoom;
        return new Rect(0, 0, zoomedWidth, zoomedHeight);
    }

    private void HandleEvents()
    {
        Event currentEvent = Event.current;

        // Define the visible area for the texture within the scroll view
        _textureAreaRect = new Rect(0, 0, texture.width * zoom, texture.height * zoom);

        // Check if the mouse is within the texture area before processing the scroll wheel event
        if (_textureAreaRect.Contains(currentEvent.mousePosition) && currentEvent.type == EventType.ScrollWheel)
        {
            float oldZoom = zoom;

            // Adjust zoom based on scroll direction
            float zoomDelta = -currentEvent.delta.y * 0.03f;
            zoom = Mathf.Clamp(zoom + zoomDelta, MinZoom, MaxZoom);

            // If zoom has changed, adjust scroll position to keep mouse position as the focal point
            if (Mathf.Abs(oldZoom - zoom) > 0.001f)
            {
                // Get mouse position relative to the start of the texture content
                Vector2 mousePosInTexture = currentEvent.mousePosition;

                // Calculate the new scroll position
                // This formula ensures the point under the mouse before the zoom stays under the mouse after the zoom
                scrollPosition += (mousePosInTexture * (zoom / oldZoom)) - mousePosInTexture;
            }

            currentEvent.Use(); // Consume the event to prevent other controls from using it
            Repaint(); // Redraw the window
        }
    }
}

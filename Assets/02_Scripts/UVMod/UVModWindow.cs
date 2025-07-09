using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class UVModWindow : EditorWindow
{
    private GameObject selectedGameObject;
    private MeshFilter meshFilter;
    private Mesh mesh;
    private Vector2[] initialUvs; // Store initial UVs when object is selected or channel changes
    private Vector2[] workingUvs; // UVs currently being displayed/modified in the editor, not directly applied to mesh until explicitly saved

    private Vector2 uvOffset = Vector2.zero;
    private Vector2 uvScale = Vector2.one;
    private int selectedUVChannel = 0; // Default to UV0
    private int selectedSubmeshIndex = -1; // -1 for all submeshes, otherwise specific index
    private Color selectedVertexColor = Color.white; // Default to white, for filtering by vertex color
    private bool useVertexColorFilter = false;

    private Texture2D uvTexturePreview;

    private int activeUVHandle = -1;
    private Vector2 dragStartMousePos;
    private Vector2 dragStartUVPos;

    private List<List<int>> uvIslands;
    private List<int> selectedUVIsslandIndices;

    private bool isDraggingSelectionRect = false;
    private Rect selectionRect;
    private Vector2 selectionRectStartPos;

    /// <summary>
    /// Opens the UV Editor Window from the Unity Editor menu.
    /// </summary>
    [MenuItem("Tools/UV Editor")]
    public static void ShowWindow()
    {
        GetWindow<UVModWindow>("UV Mod");
    }

    /// <summary>
    /// Called when the window is enabled or gains focus.
    /// </summary>
    private void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChange;
        OnSelectionChange(); // Initial setup
    }

    /// <summary>
    /// Called when the window is disabled or loses focus.
    /// </summary>
    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChange;
    }

    /// <summary>
    /// Handles changes in the Unity editor selection.
    /// </summary>
    private void OnSelectionChange()
    {
        selectedGameObject = Selection.activeGameObject;
        meshFilter = null;
        mesh = null;
        initialUvs = null;
        workingUvs = null;
        uvOffset = Vector2.zero;
        uvScale = Vector2.one;
        selectedUVChannel = 0; // Reset channel selection on new object
        selectedSubmeshIndex = -1; // Reset submesh selection
        selectedVertexColor = Color.white;
        useVertexColorFilter = false;
        uvTexturePreview = null;
        activeUVHandle = -1;
        uvIslands = null;
        selectedUVIsslandIndices = new List<int>();
        isDraggingSelectionRect = false;

        if (selectedGameObject != null)
        {
            meshFilter = selectedGameObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                mesh = meshFilter.sharedMesh;
                if (mesh != null)
                {
                    LoadUVsFromChannel(selectedUVChannel, true); // Load initial UVs and working UVs
                    DetectUVIsslands(); // Detect islands on mesh load

                    // Try to get the main texture from the material
                    Renderer renderer = selectedGameObject.GetComponent<Renderer>();
                    if (renderer != null && renderer.sharedMaterial != null && renderer.sharedMaterial.mainTexture != null)
                    {
                        uvTexturePreview = renderer.sharedMaterial.mainTexture as Texture2D;
                    }
                }
            }
        }
        Repaint(); // Redraw the window
    }

    /// <summary>
    /// Loads UVs from a specified channel into the \"initialUvs\" and \"workingUvs\" arrays.
    /// </summary>
    /// <param name="channel">The UV channel index (0 for UV0, 1 for UV1, etc.).</param>
    /// <param name="resetInitial">If true, initialUvs will be set from the mesh. Otherwise, only workingUvs is updated.</param>
    private void LoadUVsFromChannel(int channel, bool resetInitial = false)
    {
        if (mesh == null) return;

        List<Vector2> tempUvs = new List<Vector2>();
        mesh.GetUVs(channel, tempUvs);

        if (resetInitial)
        {
            initialUvs = tempUvs.ToArray();
        }
        workingUvs = tempUvs.ToArray(); // Always update workingUvs from mesh
    }

    /// <summary>
    /// Detects UV islands (connected components of UVs) in the mesh.
    /// </summary>
    private void DetectUVIsslands()
    {
        uvIslands = new List<List<int>>();
        if (mesh == null || workingUvs == null || workingUvs.Length == 0) return;

        bool[] visited = new bool[workingUvs.Length];

        // Build adjacency list for UVs based on shared triangles
        Dictionary<int, List<int>> uvAdjacency = new Dictionary<int, List<int>>();
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            uvAdjacency[i] = new List<int>();
        }

        for (int submeshIndex = 0; submeshIndex < mesh.subMeshCount; submeshIndex++)
        {
            int[] triangles = mesh.GetTriangles(submeshIndex);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int v1 = triangles[i];
                int v2 = triangles[i + 1];
                int v3 = triangles[i + 2];

                // Add connections between UVs of the same triangle
                if (!uvAdjacency[v1].Contains(v2)) uvAdjacency[v1].Add(v2);
                if (!uvAdjacency[v1].Contains(v3)) uvAdjacency[v1].Add(v3);

                if (!uvAdjacency[v2].Contains(v1)) uvAdjacency[v2].Add(v1);
                if (!uvAdjacency[v2].Contains(v3)) uvAdjacency[v2].Add(v3);

                if (!uvAdjacency[v3].Contains(v1)) uvAdjacency[v3].Add(v1);
                if (!uvAdjacency[v3].Contains(v2)) uvAdjacency[v3].Add(v2);
            }
        }

        // Perform BFS/DFS to find connected components (UV islands)
        for (int i = 0; i < workingUvs.Length; i++)
        {
            if (!visited[i])
            {
                List<int> currentIsland = new List<int>();
                Queue<int> queue = new Queue<int>();

                queue.Enqueue(i);
                visited[i] = true;

                while (queue.Count > 0)
                {
                    int currentUVIndex = queue.Dequeue();
                    currentIsland.Add(currentUVIndex);

                    foreach (int neighbor in uvAdjacency[currentUVIndex])
                    {
                        // Check if the UV coordinates are actually the same (or very close) in UV space
                        // This is crucial for distinguishing between connected vertices that are part of the same island
                        // versus vertices that are connected in 3D but separated in UV space (UV seam).
                        // For now, we are considering any vertex connected by a triangle to be part of the same island.
                        // A more robust island detection would involve checking if the UV edge is continuous.
                        if (!visited[neighbor])
                        {
                            visited[neighbor] = true;
                            queue.Enqueue(neighbor);
                        }
                    }
                }
                uvIslands.Add(currentIsland);
            }
        }
    }

    /// <summary>
    /// Called to draw and handle GUI events for the editor window.
    /// </summary>
    void OnGUI()
    {
        GUILayout.Label("UV Editor Window", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Select a 3D model in the scene to modify its UVs.", MessageType.Info);

        EditorGUILayout.ObjectField("Selected Object", selectedGameObject, typeof(GameObject), true);

        if (selectedGameObject != null && mesh != null && initialUvs != null)
        {
            EditorGUILayout.Space();
            GUILayout.Label("UV Modification", EditorStyles.boldLabel);

            // UV Channel Selection
            EditorGUI.BeginChangeCheck();
            selectedUVChannel = EditorGUILayout.IntSlider("UV Channel", selectedUVChannel, 0, 7);
            if (EditorGUI.EndChangeCheck())
            {
                LoadUVsFromChannel(selectedUVChannel, true); // Reload initial UVs on channel change
                DetectUVIsslands(); // Re-detect islands on channel change
                uvOffset = Vector2.zero;
                uvScale = Vector2.one;
            }

            // Submesh Selection
            if (mesh.subMeshCount > 1)
            {
                string[] submeshOptions = new string[mesh.subMeshCount + 1];
                submeshOptions[0] = "All Submeshes";
                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    submeshOptions[i + 1] = "Submesh " + i;
                }
                selectedSubmeshIndex = EditorGUILayout.Popup("Target Submesh", selectedSubmeshIndex + 1, submeshOptions) - 1;
            }

            // Vertex Color Filter
            useVertexColorFilter = EditorGUILayout.Toggle("Filter by Vertex Color", useVertexColorFilter);
            if (useVertexColorFilter)
            {
                selectedVertexColor = EditorGUILayout.ColorField("Target Vertex Color", selectedVertexColor);
            }

            EditorGUI.BeginChangeCheck();
            uvOffset = EditorGUILayout.Vector2Field("UV Offset", uvOffset);
            uvScale = EditorGUILayout.Vector2Field("UV Scale", uvScale);

            if (EditorGUI.EndChangeCheck())
            {
                ApplyUVChangesToWorkingUVs();
            }

            if (GUILayout.Button("Reset UVs"))
            {
                // Revert workingUvs to initialUvs
                workingUvs = (Vector2[])initialUvs.Clone();
                // Apply workingUvs to the mesh for visual feedback, but do NOT save the asset here
                if (mesh != null) // Ensure mesh is not null before setting UVs
                {
                    mesh.SetUVs(selectedUVChannel, new List<Vector2>(workingUvs));
                    EditorUtility.SetDirty(mesh); // Mark mesh as dirty so changes are reflected in inspector
                }
                uvOffset = Vector2.zero;
                uvScale = Vector2.one;
                Repaint();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Apply to Selected Objects (Batch)"))
            {
                ApplyUVChangesToMultipleObjects();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Save Modified Mesh"))
            {
                SaveMeshAsset();
                // After saving, the current state becomes the new initial state for reset
                LoadUVsFromChannel(selectedUVChannel, true);
            }

            EditorGUILayout.Space();
            GUILayout.Label("Visual UV Editor", EditorStyles.boldLabel);
            DrawUvEditorArea();
        }
        else
        {
            EditorGUILayout.LabelField("No mesh found on selected object.");
        }
    }

    /// <summary>
    /// Draws the UV editor area with the texture preview and UV overlay, and handles interactive manipulation.
    /// </summary>
    private void DrawUvEditorArea()
    {
        // Calculate a square rect for the UV editor area to maintain aspect ratio
        float desiredSize = Mathf.Min(position.width - 20, 512); // Max size 512, or window width - 20
        Rect uvEditorRect = GUILayoutUtility.GetRect(desiredSize, desiredSize, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

        // Center the square rect within the available horizontal space
        float centerX = position.width / 2f;
        uvEditorRect.x = centerX - (uvEditorRect.width / 2f);

        GUI.Box(uvEditorRect, ""); // Draw a background box

        if (uvTexturePreview != null)
        {
            // Draw the texture preview
            GUI.DrawTexture(uvEditorRect, uvTexturePreview, ScaleMode.ScaleToFit);
        }
        else
        {
            EditorGUI.HelpBox(uvEditorRect, "No texture found on material. UVs will be drawn on a blank background.", MessageType.Info);
        }

        // Handle mouse input for UV manipulation and selection
        Event currentEvent = Event.current;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        EventType eventType = currentEvent.GetTypeForControl(controlID);

        // Ensure that the event is processed only if the mouse is within the UV editor area
        if (uvEditorRect.Contains(currentEvent.mousePosition))
        {
            // Convert mouse position to UV space (0-1 range) relative to the uvEditorRect
            Vector2 mouseUVPos = new Vector2(
                (currentEvent.mousePosition.x - uvEditorRect.x) / uvEditorRect.width,
                1 - (currentEvent.mousePosition.y - uvEditorRect.y) / uvEditorRect.height // Invert Y for UV space
            );

            switch (eventType)
            {
                case EventType.MouseDown:
                    if (currentEvent.button == 0) // Left mouse button down
                    {
                        activeUVHandle = -1;
                        selectedUVIsslandIndices.Clear(); // Clear previous selection
                        float minDistance = 0.02f; // Tolerance for clicking a UV point

                        // Check if a UV point is clicked for single island selection
                        for (int i = 0; i < workingUvs.Length; i++)
                        {
                            Vector2 uvPoint = workingUvs[i];
                            if (Vector2.Distance(uvPoint, mouseUVPos) < minDistance)
                            {
                                activeUVHandle = i;
                                dragStartMousePos = currentEvent.mousePosition;
                                dragStartUVPos = workingUvs[i]; // Store current UV for relative dragging
                                Undo.RecordObject(mesh, "Drag UV Point");

                                // Select the entire UV island
                                foreach (var island in uvIslands)
                                {
                                    if (island.Contains(activeUVHandle))
                                    {
                                        selectedUVIsslandIndices.AddRange(island);
                                        break;
                                    }
                                }
                                GUIUtility.hotControl = controlID; // Capture mouse for dragging
                                currentEvent.Use();
                                return;
                            }
                        }

                        // If no UV point is clicked, start selection rectangle
                        isDraggingSelectionRect = true;
                        selectionRectStartPos = currentEvent.mousePosition;
                        selectionRect = new Rect(selectionRectStartPos.x, selectionRectStartPos.y, 0, 0);
                        GUIUtility.hotControl = controlID; // Capture mouse for dragging
                        currentEvent.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        if (activeUVHandle != -1) // Dragging a UV island
                        {
                            Vector2 deltaMousePos = currentEvent.mousePosition - dragStartMousePos;
                            Vector2 deltaUV = new Vector2(
                                deltaMousePos.x / uvEditorRect.width,
                                -deltaMousePos.y / uvEditorRect.height // Invert Y for UV space
                            );

                            // Apply the delta to all selected UV island points
                            foreach (int index in selectedUVIsslandIndices)
                            {
                                workingUvs[index] = dragStartUVPos + deltaUV; // Apply delta to initial UVs
                            }

                            // Update the mesh UVs immediately for visual feedback
                            // mesh.SetUVs(selectedUVChannel, new List<Vector2>(workingUvs)); // Removed to prevent automatic saving
                            // EditorUtility.SetDirty(mesh); // Removed to prevent automatic saving
                            Repaint();
                            currentEvent.Use();
                        }
                        else if (isDraggingSelectionRect) // Dragging selection rectangle
                        {
                            // Normalize selectionRect to handle dragging in any direction
                            selectionRect = new Rect(
                                Mathf.Min(selectionRectStartPos.x, currentEvent.mousePosition.x),
                                Mathf.Min(selectionRectStartPos.y, currentEvent.mousePosition.y),
                                Mathf.Abs(selectionRectStartPos.x - currentEvent.mousePosition.x),
                                Mathf.Abs(selectionRectStartPos.y - currentEvent.mousePosition.y)
                            );
                            Repaint();
                            currentEvent.Use();
                        }
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0; // Release mouse capture
                        if (activeUVHandle != -1) // Finished dragging a UV island
                        {
                            activeUVHandle = -1;
                            // UVs are updated in workingUvs during drag, but not applied to mesh until Save Modified Mesh is clicked.
                            // The Reset UVs button will revert workingUvs to initialUvs.
                            // No mesh.SetUVs here to prevent automatic saving.
                            // Apply workingUvs to the mesh for visual feedback, but do NOT save the asset here
                            mesh.SetUVs(selectedUVChannel, new List<Vector2>(workingUvs));
                            EditorUtility.SetDirty(mesh); // Mark mesh as dirty so changes are reflected in inspector
                            Repaint();
                            currentEvent.Use();
                        }
                        else if (isDraggingSelectionRect) // Finished dragging selection rectangle
                        {
                            isDraggingSelectionRect = false;
                            selectedUVIsslandIndices.Clear();

                            // Find UV islands that intersect with the selection rectangle
                            foreach (var island in uvIslands)
                            {
                                bool islandIntersects = false;
                                foreach (int uvIndex in island)
                                {
                                    // Convert UV point to screen space for intersection check
                                    Vector2 uvPointScreenPos = new Vector2(
                                        uvEditorRect.x + workingUvs[uvIndex].x * uvEditorRect.width,
                                        uvEditorRect.y + (1 - workingUvs[uvIndex].y) * uvEditorRect.height
                                    );

                                    if (selectionRect.Contains(uvPointScreenPos))
                                    {
                                        islandIntersects = true;
                                        break;
                                    }
                                }
                                if (islandIntersects)
                                {
                                    selectedUVIsslandIndices.AddRange(island);
                                }
                            }
                            Repaint();
                            currentEvent.Use();
                        }
                    }
                    break;

                case EventType.Repaint:
                    // Draw selection rectangle if active
                    if (isDraggingSelectionRect)
                    {
                        Handles.DrawSolidRectangleWithOutline(selectionRect, new Color(0, 0.5f, 1, 0.2f), Color.cyan);
                    }
                    break;
            }
        }

        // Draw UVs over the texture
        if (workingUvs != null && workingUvs.Length > 0)
        {
            Handles.BeginGUI();

            // Draw triangles
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                int[] triangles = mesh.GetTriangles(i);
                for (int j = 0; j < triangles.Length; j += 3)
                {
                    int v1Index = triangles[j];
                    int v2Index = triangles[j + 1];
                    int v3Index = triangles[j + 2];

                    if (v1Index < workingUvs.Length && v2Index < workingUvs.Length && v3Index < workingUvs.Length)
                    {
                        Vector2 uv1 = workingUvs[v1Index];
                        Vector2 uv2 = workingUvs[v2Index];
                        Vector2 uv3 = workingUvs[v3Index];

                        // Convert UV coordinates (0-1 range) to screen coordinates within the uvEditorRect
                        Vector3 p1 = new Vector3(uvEditorRect.x + uv1.x * uvEditorRect.width, uvEditorRect.y + (1 - uv1.y) * uvEditorRect.height, 0);
                        Vector3 p2 = new Vector3(uvEditorRect.x + uv2.x * uvEditorRect.width, uvEditorRect.y + (1 - uv2.y) * uvEditorRect.height, 0);
                        Vector3 p3 = new Vector3(uvEditorRect.x + uv3.x * uvEditorRect.width, uvEditorRect.y + (1 - uv3.y) * uvEditorRect.height, 0);

                        Handles.color = Color.red; // Default color for triangles
                        if (selectedUVIsslandIndices.Contains(v1Index) && selectedUVIsslandIndices.Contains(v2Index) && selectedUVIsslandIndices.Contains(v3Index))
                        {
                            Handles.color = Color.yellow; // Highlight selected island triangles
                        }
                        Handles.DrawLine(p1, p2);
                        Handles.DrawLine(p2, p3);
                        Handles.DrawLine(p3, p1);
                    }
                }
            }

            // Draw UV points as circles
            foreach (int index in Enumerable.Range(0, workingUvs.Length))
            {
                Vector2 uvPoint = workingUvs[index];
                Vector3 screenPos = new Vector3(uvEditorRect.x + uvPoint.x * uvEditorRect.width, uvEditorRect.y + (1 - uvPoint.y) * uvEditorRect.height, 0);

                Handles.color = Color.blue; // Default color for UV points
                if (selectedUVIsslandIndices.Contains(index))
                {
                    Handles.color = Color.green; // Highlight selected UV points
                }
                Handles.DrawSolidDisc(screenPos, Vector3.forward, 3); // Draw a small disc for each UV point
            }

            Handles.EndGUI();
        }
    }

    /// <summary>
    /// Applies the calculated UV changes (offset and scale) to the mesh of the currently selected object,
    /// considering submesh and vertex color filters.
    /// </summary>
    private void ApplyUVChangesToWorkingUVs()
    {
        if (mesh == null || initialUvs == null) return;

        // Start with initial UVs for the selected channel
        workingUvs = (Vector2[])initialUvs.Clone();

        List<int> affectedVertexIndices = new List<int>();

        // Determine affected vertices based on submesh selection
        if (selectedSubmeshIndex >= 0 && selectedSubmeshIndex < mesh.subMeshCount)
        {
            int[] triangles = mesh.GetTriangles(selectedSubmeshIndex);
            foreach (int triIndex in triangles)
            {
                if (!affectedVertexIndices.Contains(triIndex))
                {
                    affectedVertexIndices.Add(triIndex);
                }
            }
        }
        else // All submeshes
        {
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                affectedVertexIndices.Add(i);
            }
        }

        // Further filter by vertex color if enabled
        if (useVertexColorFilter && mesh.colors.Length > 0)
        {
            Color[] vertexColors = mesh.colors;
            List<int> colorFilteredIndices = new List<int>();
            foreach (int index in affectedVertexIndices)
            {
                if (index < vertexColors.Length && AreColorsApproximatelyEqual(vertexColors[index], selectedVertexColor))
                {
                    colorFilteredIndices.Add(index);
                }
            }
            affectedVertexIndices = colorFilteredIndices;
        }

        // Apply transformations only to affected vertices
        foreach (int index in affectedVertexIndices)
        {
            if (index < workingUvs.Length)
            {
                workingUvs[index] = new Vector2(initialUvs[index].x * uvScale.x + uvOffset.x, initialUvs[index].y * uvScale.y + uvOffset.y);
            }
        }
    }

    /// <summary>
    /// Applies the calculated UV changes (offset and scale) to the meshes of all currently selected objects,
    /// considering submesh and vertex color filters.
    /// </summary>
    private void ApplyUVChangesToMultipleObjects()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("No Objects Selected", "Please select one or more GameObjects to apply UV changes to.", "OK");
            return;
        }

        Undo.SetCurrentGroupName("Batch Modify UVs");
        int undoGroup = Undo.GetCurrentGroup();

        foreach (GameObject obj in selectedObjects)
        {
            MeshFilter currentMeshFilter = obj.GetComponent<MeshFilter>();
            if (currentMeshFilter != null && currentMeshFilter.sharedMesh != null)
            {
                Mesh currentMesh = currentMeshFilter.sharedMesh;
                List<Vector2> currentUvsList = new List<Vector2>();
                currentMesh.GetUVs(selectedUVChannel, currentUvsList);

                if (currentUvsList.Count > 0)
                {
                    Undo.RecordObject(currentMesh, "Modify UVs for " + obj.name);

                    Vector2[] newUvs = new Vector2[currentUvsList.Count];
                    for (int i = 0; i < currentUvsList.Count; i++)
                    {
                        newUvs[i] = new Vector2(currentUvsList[i].x * uvScale.x + uvOffset.x, currentUvsList[i].y * uvScale.y + uvOffset.y);
                    }
                    currentMesh.SetUVs(selectedUVChannel, new List<Vector2>(newUvs));
                    EditorUtility.SetDirty(currentMesh);
                }
            }
        }
        Undo.CollapseUndoOperations(undoGroup);
        EditorUtility.DisplayDialog("Batch UV Modification", selectedObjects.Length + " objects processed.", "OK");
    }

    /// <summary>
    /// Saves the modified mesh as a new asset or updates an existing one.
    /// </summary>
    private void SaveMeshAsset()
    {
        if (mesh == null)
        {
            EditorUtility.DisplayDialog("Error", "No mesh to save.", "OK");
            return;
        }

        string path = AssetDatabase.GetAssetPath(mesh);
        if (string.IsNullOrEmpty(path))
        {
            path = EditorUtility.SaveFilePanelInProject("Save Modified Mesh", selectedGameObject.name + "_Modified", "asset", "Save the modified mesh as a new asset.");
            if (string.IsNullOrEmpty(path))
            {
                return; // User cancelled save operation
            }

            Mesh newMesh = new Mesh();
            newMesh.vertices = mesh.vertices;
            newMesh.triangles = mesh.triangles;
            newMesh.normals = mesh.normals;
            newMesh.tangents = mesh.tangents;

            for (int i = 0; i < 8; i++)
            {
                List<Vector2> tempUvs = new List<Vector2>();
                mesh.GetUVs(i, tempUvs);
                if (tempUvs.Count > 0)
                {
                    newMesh.SetUVs(i, tempUvs);
                }
            }

            newMesh.colors = mesh.colors;
            newMesh.name = mesh.name + "_Modified";

            AssetDatabase.CreateAsset(newMesh, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            meshFilter.mesh = newMesh;
            EditorUtility.DisplayDialog("Success", "Mesh saved as new asset: " + path, "OK");
        }
        else
        {
            // Apply workingUvs to the mesh before saving
            mesh.SetUVs(selectedUVChannel, new List<Vector2>(workingUvs));
            EditorUtility.SetDirty(mesh);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Success", "Mesh asset updated: " + path, "OK");
        }
    }

    /// <summary>
    /// Compares two colors for approximate equality, useful for floating-point color values.
    /// </summary>
    private bool AreColorsApproximatelyEqual(Color c1, Color c2, float tolerance = 0.01f)
    {
        return Mathf.Abs(c1.r - c2.r) < tolerance &&
               Mathf.Abs(c1.g - c2.g) < tolerance &&
               Mathf.Abs(c1.b - c2.b) < tolerance &&
               Mathf.Abs(c1.a - c2.a) < tolerance;
    }
}


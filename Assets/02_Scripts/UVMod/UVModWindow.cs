using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class UVModWindow : EditorWindow
{
    // Mesh and UV Data
    private GameObject selectedGameObject;
    private MeshFilter meshFilter;
    private Mesh mesh;
    private Vector2[] initialUvs;
    private Vector2[] workingUvs;
    private Dictionary<int, Vector2> dragStartIslandUVs;

    // UV Modification Settings
    private Vector2 uvOffset = Vector2.zero;
    private Vector2 uvScale = Vector2.one;
    private int selectedUVChannel = 0;
    private int selectedSubmeshIndex = -1;
    private Color selectedVertexColor = Color.white;
    private bool useVertexColorFilter = false;

    // UV Editor State
    private Texture2D uvTexturePreview;
    private List<List<int>> uvIslands;
    private List<int> selectedUVIsslandIndices = new List<int>();

    // Interaction State
    private int activeUVHandle = -1;
    private Vector2 dragStartMousePos;
    private bool isDraggingSelectionRect = false;
    private Rect selectionRect;
    private Vector2 selectionRectStartPos;

    // Viewport Pan and Zoom State
    private float _zoom = 1.0f;
    private Vector2 _scrollPosition = Vector2.zero;
    private bool isPanning = false;
    private Vector2 panStartMousePos;
    private const float MinZoom = 0.1f;
    private const float MaxZoom = 20.0f;


    [MenuItem("Tools/UV Editor")]
    public static void ShowWindow()
    {
        GetWindow<UVModWindow>("UV Mod");
    }

    private void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChange;
        OnSelectionChange();
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChange;
    }

    private void OnSelectionChange()
    {
        if (mesh != null && initialUvs != null && workingUvs != null && !Enumerable.SequenceEqual(initialUvs, workingUvs))
        {
            if (EditorUtility.DisplayDialog("Unsaved UV Changes",
               "You have unsaved UV modifications. Would you like to revert them?", "Revert", "Keep"))
            {
                ResetUVs();
            }
        }

        selectedGameObject = Selection.activeGameObject;
        meshFilter = null;
        mesh = null;
        initialUvs = null;
        workingUvs = null;
        uvOffset = Vector2.zero;
        uvScale = Vector2.one;
        selectedUVChannel = 0;
        selectedSubmeshIndex = -1;
        selectedVertexColor = Color.white;
        useVertexColorFilter = false;
        uvTexturePreview = null;
        activeUVHandle = -1;
        uvIslands = null;
        selectedUVIsslandIndices = new List<int>();
        isDraggingSelectionRect = false;
        _zoom = 1.0f;
        _scrollPosition = Vector2.zero;

        if (selectedGameObject != null)
        {
            meshFilter = selectedGameObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                mesh = meshFilter.sharedMesh;
                if (mesh != null)
                {
                    LoadUVsFromChannel(selectedUVChannel, true);
                    DetectUVIsslands();

                    Renderer renderer = selectedGameObject.GetComponent<Renderer>();
                    if (renderer != null && renderer.sharedMaterial != null && renderer.sharedMaterial.mainTexture != null)
                    {
                        uvTexturePreview = renderer.sharedMaterial.mainTexture as Texture2D;
                    }
                }
            }
        }
        Repaint();
    }

    #region Data Loading and Setup
    private void ResetUVs()
    {
        if (initialUvs == null || mesh == null) return;
        workingUvs = (Vector2[])initialUvs.Clone();
        mesh.SetUVs(selectedUVChannel, new List<Vector2>(workingUvs));
        EditorUtility.SetDirty(mesh);
        uvOffset = Vector2.zero;
        uvScale = Vector2.one;
        Repaint();
    }

    private void LoadUVsFromChannel(int channel, bool resetInitial = false)
    {
        if (mesh == null) return;
        List<Vector2> tempUvs = new List<Vector2>();
        mesh.GetUVs(channel, tempUvs);
        if (resetInitial)
        {
            initialUvs = tempUvs.ToArray();
        }
        workingUvs = tempUvs.ToArray();
    }

    private void DetectUVIsslands()
    {
        uvIslands = new List<List<int>>();
        if (mesh == null || workingUvs == null || workingUvs.Length == 0) return;

        var visited = new bool[mesh.vertexCount];
        var adj = new Dictionary<int, List<int>>();
        for (int i = 0; i < mesh.vertexCount; i++) adj[i] = new List<int>();

        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 3)
            {
                int v1 = triangles[j], v2 = triangles[j + 1], v3 = triangles[j + 2];
                adj[v1].Add(v2); adj[v1].Add(v3);
                adj[v2].Add(v1); adj[v2].Add(v3);
                adj[v3].Add(v1); adj[v3].Add(v2);
            }
        }

        for (int i = 0; i < mesh.vertexCount; i++)
        {
            if (!visited[i])
            {
                var island = new List<int>();
                var q = new Queue<int>();
                q.Enqueue(i);
                visited[i] = true;
                while (q.Count > 0)
                {
                    int u = q.Dequeue();
                    island.Add(u);
                    foreach (int v in adj[u].Distinct())
                    {
                        if (!visited[v])
                        {
                            visited[v] = true;
                            q.Enqueue(v);
                        }
                    }
                }
                uvIslands.Add(island);
            }
        }
    }
    #endregion

    void OnGUI()
    {
        GUILayout.Label("UV Editor Window", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Select a 3D model. Use Scroll Wheel to Zoom. Use Scrollbars or Middle-Mouse-Drag to Pan.", MessageType.Info);

        EditorGUILayout.ObjectField("Selected Object", selectedGameObject, typeof(GameObject), true);

        if (selectedGameObject != null && mesh != null && initialUvs != null)
        {
            DrawSettingsGUI();

            EditorGUILayout.Space();
            GUILayout.Label("Visual UV Editor", EditorStyles.boldLabel);
            DrawUvEditorArea();
        }
        else
        {
            EditorGUILayout.LabelField("No mesh found on selected object.");
        }
    }

    private void DrawSettingsGUI()
    {
        EditorGUILayout.Space();
        GUILayout.Label("UV Modification", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        selectedUVChannel = EditorGUILayout.IntSlider("UV Channel", selectedUVChannel, 0, 7);
        if (EditorGUI.EndChangeCheck())
        {
            LoadUVsFromChannel(selectedUVChannel, true);
            DetectUVIsslands();
            uvOffset = Vector2.zero;
            uvScale = Vector2.one;
        }

        if (mesh.subMeshCount > 1)
        {
            string[] submeshOptions = new string[mesh.subMeshCount + 1];
            submeshOptions[0] = "All Submeshes";
            for (int i = 0; i < mesh.subMeshCount; i++) submeshOptions[i + 1] = "Submesh " + i;
            selectedSubmeshIndex = EditorGUILayout.Popup("Target Submesh", selectedSubmeshIndex + 1, submeshOptions) - 1;
        }

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

        if (GUILayout.Button("Reset UVs")) ResetUVs();
        EditorGUILayout.Space();
        if (GUILayout.Button("Apply to Selected Objects (Batch)")) ApplyUVChangesToMultipleObjects();
        EditorGUILayout.Space();
        if (GUILayout.Button("Save Modified Mesh"))
        {
            SaveMeshAsset();
            LoadUVsFromChannel(selectedUVChannel, true);
        }
    }

    private void DrawUvEditorArea()
    {
        // *** THE FIX: Use a single, contained block for layout and drawing. ***
        // This group provides a flexible, framed area for the editor.
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        {
            // Reserve a square area that fills the available space.
            Rect viewRect = GUILayoutUtility.GetAspectRect(1.0f);

            // The rect is now valid for all event types within this OnGUI call.
            if (viewRect.width > 0 && viewRect.height > 0)
            {
                Rect contentRect = new Rect(0, 0, viewRect.width * _zoom, viewRect.height * _zoom);

                HandleEvents(viewRect, contentRect);

                _scrollPosition = GUI.BeginScrollView(viewRect, _scrollPosition, contentRect, false, false);
                {
                    if (uvTexturePreview != null)
                    {
                        GUI.DrawTexture(new Rect(0, 0, contentRect.width, contentRect.height), uvTexturePreview, ScaleMode.StretchToFill);
                    }

                    if (workingUvs != null && workingUvs.Length > 0)
                    {
                        GUI.BeginGroup(new Rect(0, 0, contentRect.width, contentRect.height));
                        {
                            for (int i = 0; i < mesh.subMeshCount; i++)
                            {
                                int[] triangles = mesh.GetTriangles(i);
                                for (int j = 0; j < triangles.Length; j += 3)
                                {
                                    int v1 = triangles[j], v2 = triangles[j + 1], v3 = triangles[j + 2];
                                    if (v1 < workingUvs.Length && v2 < workingUvs.Length && v3 < workingUvs.Length)
                                    {
                                        Vector3 p1 = ConvertUVToContentPos(workingUvs[v1], contentRect);
                                        Vector3 p2 = ConvertUVToContentPos(workingUvs[v2], contentRect);
                                        Vector3 p3 = ConvertUVToContentPos(workingUvs[v3], contentRect);

                                        Handles.color = selectedUVIsslandIndices.Contains(v1) ? Color.yellow : new Color(1, 0.3f, 0.3f, 0.8f);
                                        Handles.DrawLine(p1, p2);
                                        Handles.DrawLine(p2, p3);
                                        Handles.DrawLine(p3, p1);
                                    }
                                }
                            }

                            for (int index = 0; index < workingUvs.Length; index++)
                            {
                                Vector3 handlePos = ConvertUVToContentPos(workingUvs[index], contentRect);
                                Handles.color = selectedUVIsslandIndices.Contains(index) ? Color.green : Color.cyan;
                                Handles.DrawSolidDisc(handlePos, Vector3.forward, 3f);
                            }
                        }
                        GUI.EndGroup();
                    }
                }
                GUI.EndScrollView();
            }
        }
        GUILayout.EndVertical();

        if (isDraggingSelectionRect)
        {
            Handles.BeginGUI();
            Handles.DrawSolidRectangleWithOutline(selectionRect, new Color(0, 0.5f, 1, 0.2f), Color.cyan);
            Handles.EndGUI();
        }
    }

    private void HandleEvents(Rect viewRect, Rect contentRect)
    {
        Event currentEvent = Event.current;

        if (!viewRect.Contains(currentEvent.mousePosition))
        {
            if (currentEvent.type != EventType.MouseUp && currentEvent.type != EventType.MouseDrag)
            {
                return;
            }
        }

        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        EventType eventType = currentEvent.GetTypeForControl(controlID);

        if (viewRect.Contains(currentEvent.mousePosition))
        {
            if (currentEvent.type == EventType.ScrollWheel)
            {
                float oldZoom = _zoom;
                float zoomDelta = -currentEvent.delta.y * 0.05f;
                _zoom = Mathf.Clamp(_zoom + zoomDelta * _zoom, MinZoom, MaxZoom);

                Vector2 mousePosInView = currentEvent.mousePosition - viewRect.position;
                _scrollPosition = ((_scrollPosition + mousePosInView) * (_zoom / oldZoom)) - mousePosInView;

                Repaint();
                currentEvent.Use();
                return;
            }

            if (eventType == EventType.MouseDown)
            {
                if (currentEvent.button == 2)
                {
                    isPanning = true;
                    panStartMousePos = currentEvent.mousePosition;
                    GUIUtility.hotControl = controlID;
                    currentEvent.Use();
                    return;
                }

                if (currentEvent.button == 0)
                {
                    bool isVerticalScrollbarVisible = contentRect.height > viewRect.height;
                    bool isHorizontalScrollbarVisible = contentRect.width > viewRect.width;

                    Rect verticalScrollbarRect = new Rect(viewRect.x + viewRect.width - GUI.skin.verticalScrollbar.fixedWidth, viewRect.y, GUI.skin.verticalScrollbar.fixedWidth, viewRect.height);
                    Rect horizontalScrollbarRect = new Rect(viewRect.x, viewRect.y + viewRect.height - GUI.skin.horizontalScrollbar.fixedHeight, viewRect.width, GUI.skin.horizontalScrollbar.fixedHeight);

                    if ((isVerticalScrollbarVisible && verticalScrollbarRect.Contains(currentEvent.mousePosition)) ||
                        (isHorizontalScrollbarVisible && horizontalScrollbarRect.Contains(currentEvent.mousePosition)))
                    {
                        return;
                    }

                    GUIUtility.hotControl = controlID;
                    activeUVHandle = -1;
                    Vector2 mouseUVPos = ConvertScreenPosToUVPos(currentEvent.mousePosition, viewRect);
                    float minDistance = (0.02f / _zoom);

                    int topHandle = -1;
                    float minD = float.MaxValue;
                    for (int i = 0; i < workingUvs.Length; i++)
                    {
                        float d = Vector2.Distance(workingUvs[i], mouseUVPos);
                        if (d < minDistance && d < minD)
                        {
                            minD = d;
                            topHandle = i;
                        }
                    }

                    if (topHandle != -1)
                    {
                        activeUVHandle = topHandle;
                        dragStartMousePos = currentEvent.mousePosition;
                        Undo.RecordObject(mesh, "Drag UV Point");

                        dragStartIslandUVs = new Dictionary<int, Vector2>();
                        if (!selectedUVIsslandIndices.Contains(activeUVHandle))
                        {
                            if (!currentEvent.shift) selectedUVIsslandIndices.Clear();
                            foreach (var island in uvIslands)
                            {
                                if (island.Contains(activeUVHandle))
                                {
                                    selectedUVIsslandIndices.AddRange(island.Except(selectedUVIsslandIndices));
                                    break;
                                }
                            }
                        }
                        foreach (int index in selectedUVIsslandIndices)
                        {
                            dragStartIslandUVs[index] = workingUvs[index];
                        }
                    }
                    else
                    {
                        isDraggingSelectionRect = true;
                        selectionRectStartPos = currentEvent.mousePosition;
                        selectionRect = new Rect(selectionRectStartPos.x, selectionRectStartPos.y, 0, 0);
                    }
                    currentEvent.Use();
                }
            }
        }

        if (eventType == EventType.MouseDrag && GUIUtility.hotControl == controlID)
        {
            if (isPanning)
            {
                Vector2 delta = currentEvent.mousePosition - panStartMousePos;
                _scrollPosition -= delta;
                panStartMousePos = currentEvent.mousePosition;
            }
            else if (activeUVHandle != -1)
            {
                Vector2 startMouseUV = ConvertScreenPosToUVPos(dragStartMousePos, viewRect);
                Vector2 currentMouseUV = ConvertScreenPosToUVPos(currentEvent.mousePosition, viewRect);
                Vector2 deltaUV = currentMouseUV - startMouseUV;

                foreach (var islandKvp in dragStartIslandUVs)
                {
                    workingUvs[islandKvp.Key] = islandKvp.Value + deltaUV;
                }
            }
            else if (isDraggingSelectionRect)
            {
                selectionRect = new Rect(
                    Mathf.Min(selectionRectStartPos.x, currentEvent.mousePosition.x),
                    Mathf.Min(selectionRectStartPos.y, currentEvent.mousePosition.y),
                    Mathf.Abs(selectionRectStartPos.x - currentEvent.mousePosition.x),
                    Mathf.Abs(selectionRectStartPos.y - currentEvent.mousePosition.y)
                );
            }
            Repaint();
            currentEvent.Use();
        }

        if (eventType == EventType.MouseUp && GUIUtility.hotControl == controlID)
        {
            GUIUtility.hotControl = 0;
            isPanning = false;
            activeUVHandle = -1;
            dragStartIslandUVs = null;

            if (isDraggingSelectionRect)
            {
                isDraggingSelectionRect = false;
                if (!currentEvent.shift) selectedUVIsslandIndices.Clear();

                foreach (var island in uvIslands)
                {
                    bool islandIntersects = island.Any(uvIndex => selectionRect.Contains(ConvertUVToScreenPos(workingUvs[uvIndex], viewRect)));
                    if (islandIntersects)
                    {
                        selectedUVIsslandIndices.AddRange(island.Except(selectedUVIsslandIndices));
                    }
                }
            }
            else
            {
                mesh.SetUVs(selectedUVChannel, new List<Vector2>(workingUvs));
                EditorUtility.SetDirty(mesh);
            }
            Repaint();
            currentEvent.Use();
        }
    }

    #region Coordinate Conversion Helpers
    private Vector2 ConvertScreenPosToUVPos(Vector2 screenPos, Rect viewRect)
    {
        Vector2 localPos = screenPos - viewRect.position;
        Vector2 posInContent = localPos + _scrollPosition;
        return new Vector2(posInContent.x / (viewRect.width * _zoom), 1 - posInContent.y / (viewRect.height * _zoom));
    }

    private Vector3 ConvertUVToContentPos(Vector2 uvPos, Rect contentRect)
    {
        return new Vector3(uvPos.x * contentRect.width, (1 - uvPos.y) * contentRect.height, 0);
    }

    private Vector3 ConvertUVToScreenPos(Vector2 uvPos, Rect viewRect)
    {
        Vector2 posInContent = new Vector2(uvPos.x * (viewRect.width * _zoom), (1 - uvPos.y) * (viewRect.height * _zoom));
        Vector2 screenPos = posInContent - _scrollPosition + viewRect.position;
        return new Vector3(screenPos.x, screenPos.y, 0);
    }
    #endregion

    #region Data Modification and Saving
    private void ApplyUVChangesToWorkingUVs()
    {
        if (mesh == null || initialUvs == null) return;
        workingUvs = (Vector2[])initialUvs.Clone();
        List<int> affectedIndices = new List<int>();
        if (selectedSubmeshIndex >= 0 && selectedSubmeshIndex < mesh.subMeshCount)
        {
            affectedIndices.AddRange(mesh.GetTriangles(selectedSubmeshIndex).Distinct());
        }
        else
        {
            affectedIndices.AddRange(Enumerable.Range(0, mesh.vertexCount));
        }

        if (useVertexColorFilter && mesh.colors.Length > 0)
        {
            Color[] vertexColors = mesh.colors;
            affectedIndices = affectedIndices.Where(index => index < vertexColors.Length && AreColorsApproximatelyEqual(vertexColors[index], selectedVertexColor)).ToList();
        }

        foreach (int index in affectedIndices)
        {
            if (index < workingUvs.Length)
            {
                workingUvs[index] = new Vector2(initialUvs[index].x * uvScale.x + uvOffset.x, initialUvs[index].y * uvScale.y + uvOffset.y);
            }
        }
        mesh.SetUVs(selectedUVChannel, workingUvs);
        EditorUtility.SetDirty(mesh);
        Repaint();
    }

    private void ApplyUVChangesToMultipleObjects()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("No Objects Selected", "Please select one or more GameObjects.", "OK");
            return;
        }
        Undo.SetCurrentGroupName("Batch Modify UVs");
        int undoGroup = Undo.GetCurrentGroup();
        foreach (GameObject obj in selectedObjects)
        {
            MeshFilter mf = obj.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                Mesh m = mf.sharedMesh;
                List<Vector2> uvs = new List<Vector2>();
                m.GetUVs(selectedUVChannel, uvs);
                if (uvs.Count > 0)
                {
                    Undo.RecordObject(m, "Modify UVs for " + obj.name);
                    Vector2[] newUvs = uvs.Select(uv => new Vector2(uv.x * uvScale.x + uvOffset.x, uv.y * uvScale.y + uvOffset.y)).ToArray();
                    m.SetUVs(selectedUVChannel, newUvs);
                    EditorUtility.SetDirty(m);
                }
            }
        }
        Undo.CollapseUndoOperations(undoGroup);
        EditorUtility.DisplayDialog("Batch UV Modification", selectedObjects.Length + " objects processed.", "OK");
    }

    private void SaveMeshAsset()
    {
        if (mesh == null) return;
        string path = AssetDatabase.GetAssetPath(mesh);
        if (string.IsNullOrEmpty(path) || !AssetDatabase.IsMainAsset(mesh))
        {
            path = EditorUtility.SaveFilePanelInProject("Save Modified Mesh", selectedGameObject.name + "_Modified", "asset", "Save the modified mesh.");
            if (string.IsNullOrEmpty(path)) return;

            Mesh newMesh = Instantiate(mesh);
            newMesh.name = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(newMesh, path);
            meshFilter.mesh = newMesh;
        }

        mesh.SetUVs(selectedUVChannel, workingUvs);
        EditorUtility.SetDirty(mesh);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Success", "Mesh asset saved/updated at: " + path, "OK");
    }

    private bool AreColorsApproximatelyEqual(Color c1, Color c2, float tolerance = 0.01f)
    {
        return Mathf.Abs(c1.r - c2.r) < tolerance &&
               Mathf.Abs(c1.g - c2.g) < tolerance &&
               Mathf.Abs(c1.b - c2.b) < tolerance &&
               Mathf.Abs(c1.a - c2.a) < tolerance;
    }
    #endregion
}

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

    // --- Transform States ---
    // Global transforms (when nothing is selected)
    private Vector2 uvOffset = Vector2.zero;
    private Vector2 uvScale = Vector2.one;
    // Per-island transforms (when islands are selected)
    private Vector2 islandTransformOffset = Vector2.zero;
    private Vector2 islandTransformScale = Vector2.one;
    private Dictionary<int, Vector2> selectionStartUVs; // Snapshot of UVs at the moment of selection
    private Vector2 selectionCenter; // Pivot for scaling selected islands

    // UV Modification Settings
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
    private bool isPickingColor = false;

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
        isPickingColor = false;
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
        DrawSettingsGUI();

        EditorGUILayout.Space();
        GUILayout.Label("Visual UV Editor", EditorStyles.boldLabel);

        DrawUvEditorArea();
    }

    private void DrawSettingsGUI()
    {
        GUILayout.Label("UV Editor Window", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Select a 3D model. Use Scroll Wheel to Zoom. Use Scrollbars or Middle-Mouse-Drag to Pan.", MessageType.Info);
        EditorGUILayout.ObjectField("Selected Object", selectedGameObject, typeof(GameObject), true);

        if (selectedGameObject == null || mesh == null) return;

        // --- Global Controls ---
        if (selectedUVIsslandIndices.Count == 0)
        {
            GUI.enabled = selectedUVIsslandIndices.Count == 0;
            EditorGUILayout.Space();
            GUILayout.Label("Global UV Modification", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            uvOffset = EditorGUILayout.Vector2Field("UV Offset", uvOffset);
            uvScale = EditorGUILayout.Vector2Field("UV Scale", uvScale);
            if (EditorGUI.EndChangeCheck())
            {
                ApplyGlobalUVChanges();
            }
        }        
        // --- Per-Island Controls ---
        else
        {
            EditorGUILayout.Space();
            GUILayout.Label("Selected Islands Transform", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            islandTransformOffset = EditorGUILayout.Vector2Field("Offset", islandTransformOffset);
            islandTransformScale = EditorGUILayout.Vector2Field("Scale", islandTransformScale);
            if (EditorGUI.EndChangeCheck())
            {
                ApplyIslandTransforms();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply")) CommitIslandTransforms();
            if (GUILayout.Button("Reset")) ResetIslandTransforms();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        GUILayout.Label("Settings & Tools", EditorStyles.boldLabel);

        selectedUVChannel = EditorGUILayout.IntSlider("UV Channel", selectedUVChannel, 0, 7);
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
            EditorGUILayout.BeginHorizontal();
            {
                selectedVertexColor = EditorGUILayout.ColorField("Target Vertex Color", selectedVertexColor);
                if (selectedUVIsslandIndices.Count > 0)
                {
                    GUI.enabled = uvTexturePreview != null;
                    if (GUILayout.Button("Pick & Apply Color", GUILayout.Width(140)))
                    {
                        isPickingColor = true;
                    }
                    GUI.enabled = true;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Reset All UVs")) ResetUVs();
        if (GUILayout.Button("Save Modified Mesh"))
        {
            SaveMeshAsset();
            LoadUVsFromChannel(selectedUVChannel, true);
        }
    }

    private void DrawUvEditorArea()
    {
        Rect viewRect = GUILayoutUtility.GetRect(100, 100, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        Rect contentRect = new Rect(0, 0, viewRect.width * _zoom, viewRect.height * _zoom);

        if (isPickingColor)
        {
            EditorGUI.DrawRect(viewRect, new Color(0, 1, 1, 0.2f));
            EditorGUIUtility.AddCursorRect(viewRect, MouseCursor.Arrow);
        }
        else
        {
            GUI.Box(viewRect, GUIContent.none);
        }

        HandleEvents(viewRect, contentRect);

        _scrollPosition = GUI.BeginScrollView(viewRect, _scrollPosition, contentRect, true, true);
        {
            Rect textureDisplayRect = CalculateAspectRatioRect(contentRect, uvTexturePreview);

            if (uvTexturePreview != null)
            {
                GUI.DrawTexture(textureDisplayRect, uvTexturePreview, ScaleMode.ScaleToFit);
            }

            if (workingUvs != null && workingUvs.Length > 0)
            {
                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    int[] triangles = mesh.GetTriangles(i);
                    for (int j = 0; j < triangles.Length; j += 3)
                    {
                        int v1 = triangles[j], v2 = triangles[j + 1], v3 = triangles[j + 2];
                        if (v1 < workingUvs.Length && v2 < workingUvs.Length && v3 < workingUvs.Length)
                        {
                            Vector3 p1 = ConvertUVToContentPos(workingUvs[v1], textureDisplayRect);
                            Vector3 p2 = ConvertUVToContentPos(workingUvs[v2], textureDisplayRect);
                            Vector3 p3 = ConvertUVToContentPos(workingUvs[v3], textureDisplayRect);

                            Handles.color = selectedUVIsslandIndices.Contains(v1) ? Color.yellow : new Color(1, 0.3f, 0.3f, 0.8f);
                            Handles.DrawLine(p1, p2);
                            Handles.DrawLine(p2, p3);
                            Handles.DrawLine(p3, p1);
                        }
                    }
                }

                for (int index = 0; index < workingUvs.Length; index++)
                {
                    Vector3 handlePos = ConvertUVToContentPos(workingUvs[index], textureDisplayRect);

                    Color handleColor;
                    bool isSelected = selectedUVIsslandIndices.Contains(index);

                    if (isSelected)
                    {
                        handleColor = Color.cyan;
                    }
                    else if (mesh.colors.Length == mesh.vertexCount && mesh.colors[index].a > 0)
                    {
                        handleColor = mesh.colors[index];
                    }
                    else
                    {
                        handleColor = Color.blue;
                    }

                    Handles.color = handleColor;
                    Handles.DrawSolidDisc(handlePos, Vector3.forward, 3f);
                }
            }
        }
        GUI.EndScrollView();

        if (isDraggingSelectionRect)
        {
            Handles.BeginGUI();
            Handles.DrawSolidRectangleWithOutline(selectionRect, new Color(0, 1f, 1f, 0.2f), new Color(0, 1f, 1f, 1f));
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
                bool isVerticalScrollbarVisible = contentRect.height > viewRect.height;
                bool isHorizontalScrollbarVisible = contentRect.width > viewRect.width;
                Rect verticalScrollbarRect = new Rect(viewRect.x + viewRect.width - GUI.skin.verticalScrollbar.fixedWidth, viewRect.y, GUI.skin.verticalScrollbar.fixedWidth, viewRect.height);
                Rect horizontalScrollbarRect = new Rect(viewRect.x, viewRect.y + viewRect.height - GUI.skin.horizontalScrollbar.fixedHeight, viewRect.width, GUI.skin.horizontalScrollbar.fixedHeight);
                if ((isVerticalScrollbarVisible && verticalScrollbarRect.Contains(currentEvent.mousePosition)) ||
                    (isHorizontalScrollbarVisible && horizontalScrollbarRect.Contains(currentEvent.mousePosition)))
                {
                    return;
                }

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
                    if (isPickingColor)
                    {
                        PickAndApplyVertexColor(currentEvent.mousePosition, viewRect, contentRect);
                        isPickingColor = false;
                        GUIUtility.hotControl = controlID;
                        currentEvent.Use();
                        return;
                    }

                    GUIUtility.hotControl = controlID;
                    activeUVHandle = -1;
                    Rect textureDisplayRect = CalculateAspectRatioRect(contentRect, uvTexturePreview);
                    Vector2 mouseUVPos = ConvertScreenPosToUVPos(currentEvent.mousePosition, viewRect, textureDisplayRect);
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

                        if (!selectedUVIsslandIndices.Contains(activeUVHandle))
                        {
                            if (!currentEvent.shift)
                            {
                                selectedUVIsslandIndices.Clear();
                            }
                            foreach (var island in uvIslands)
                            {
                                if (island.Contains(activeUVHandle))
                                {
                                    selectedUVIsslandIndices.AddRange(island.Except(selectedUVIsslandIndices));
                                    break;
                                }
                            }
                            CaptureSelectionState();
                        }

                        dragStartIslandUVs = new Dictionary<int, Vector2>();
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
                Rect textureDisplayRect = CalculateAspectRatioRect(contentRect, uvTexturePreview);
                Vector2 startMouseUV = ConvertScreenPosToUVPos(dragStartMousePos, viewRect, textureDisplayRect);
                Vector2 currentMouseUV = ConvertScreenPosToUVPos(currentEvent.mousePosition, viewRect, textureDisplayRect);
                Vector2 deltaUV = currentMouseUV - startMouseUV;

                foreach (var islandKvp in dragStartIslandUVs)
                {
                    workingUvs[islandKvp.Key] = islandKvp.Value + deltaUV;
                }
                CaptureSelectionState(); // Recapture state to update pivot while dragging
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

            if (isDraggingSelectionRect)
            {
                isDraggingSelectionRect = false;
                if (!currentEvent.shift) selectedUVIsslandIndices.Clear();

                Rect textureDisplayRect = CalculateAspectRatioRect(contentRect, uvTexturePreview);
                foreach (var island in uvIslands)
                {
                    bool islandIntersects = island.Any(uvIndex => selectionRect.Contains(ConvertUVToScreenPos(workingUvs[uvIndex], viewRect, textureDisplayRect)));
                    if (islandIntersects)
                    {
                        selectedUVIsslandIndices.AddRange(island.Except(selectedUVIsslandIndices));
                    }
                }
                CaptureSelectionState();
            }
            else if (activeUVHandle != -1)
            {
                mesh.SetUVs(selectedUVChannel, new List<Vector2>(workingUvs));
                EditorUtility.SetDirty(mesh);
                CaptureSelectionState();
            }

            activeUVHandle = -1;
            dragStartIslandUVs = null;
            Repaint();
            currentEvent.Use();
        }
    }

    #region Coordinate Conversion and Color Picking

    private void PickAndApplyVertexColor(Vector2 screenPos, Rect viewRect, Rect contentRect)
    {
        if (uvTexturePreview == null || selectedUVIsslandIndices.Count == 0) return;

        if (!uvTexturePreview.isReadable)
        {
            EditorUtility.DisplayDialog("Texture Not Readable", "The texture '" + uvTexturePreview.name + "' is not marked as readable. Please enable 'Read/Write Enabled' in its import settings.", "OK");
            return;
        }

        Rect textureDisplayRect = CalculateAspectRatioRect(contentRect, uvTexturePreview);
        Vector2 pickedUV = ConvertScreenPosToUVPos(screenPos, viewRect, textureDisplayRect);

        pickedUV.x = Mathf.Clamp01(pickedUV.x);
        pickedUV.y = Mathf.Clamp01(pickedUV.y);

        selectedVertexColor = uvTexturePreview.GetPixelBilinear(pickedUV.x, pickedUV.y);

        Color[] vertexColors = mesh.colors;
        if (vertexColors.Length != mesh.vertexCount)
        {
            vertexColors = new Color[mesh.vertexCount];
            for (int i = 0; i < vertexColors.Length; i++) vertexColors[i] = Color.white;
        }

        Undo.RecordObject(mesh, "Apply Vertex Color");

        foreach (int vertIndex in selectedUVIsslandIndices)
        {
            if (vertIndex < vertexColors.Length)
            {
                vertexColors[vertIndex] = selectedVertexColor;
            }
        }

        mesh.colors = vertexColors;
        EditorUtility.SetDirty(mesh);
        Repaint();
    }

    private Rect CalculateAspectRatioRect(Rect container, Texture2D texture)
    {
        if (texture == null) return container;

        float containerAspect = container.width / container.height;
        float textureAspect = (float)texture.width / texture.height;

        Rect result = new Rect(container);

        if (containerAspect > textureAspect)
        {
            result.width = container.height * textureAspect;
            result.x = container.x + (container.width - result.width) / 2f;
        }
        else
        {
            result.height = container.width / textureAspect;
            result.y = container.y + (container.height - result.height) / 2f;
        }
        return result;
    }

    private Vector2 ConvertScreenPosToUVPos(Vector2 screenPos, Rect viewRect, Rect textureDisplayRect)
    {
        Vector2 localPos = screenPos - viewRect.position;
        Vector2 posInContent = localPos + _scrollPosition;

        Vector2 posInTexture = posInContent - textureDisplayRect.position;

        return new Vector2(posInTexture.x / textureDisplayRect.width, 1 - posInTexture.y / textureDisplayRect.height);
    }

    private Vector3 ConvertUVToContentPos(Vector2 uvPos, Rect textureDisplayRect)
    {
        float x = textureDisplayRect.x + uvPos.x * textureDisplayRect.width;
        float y = textureDisplayRect.y + (1 - uvPos.y) * textureDisplayRect.height;
        return new Vector3(x, y, 0);
    }

    private Vector3 ConvertUVToScreenPos(Vector2 uvPos, Rect viewRect, Rect textureDisplayRect)
    {
        Vector2 posInContent = new Vector2(
            textureDisplayRect.x + uvPos.x * textureDisplayRect.width,
            textureDisplayRect.y + (1 - uvPos.y) * textureDisplayRect.height
        );
        Vector2 screenPos = posInContent - _scrollPosition + viewRect.position;
        return new Vector3(screenPos.x, screenPos.y, 0);
    }
    #endregion

    #region Data Modification and Saving
    private void ApplyGlobalUVChanges()
    {
        if (mesh == null || initialUvs == null) return;

        // Re-clone initialUvs to ensure changes are not cumulative
        workingUvs = (Vector2[])initialUvs.Clone();

        // Determine which vertices to affect
        List<int> affectedIndices = new List<int>();
        if (selectedSubmeshIndex >= 0 && selectedSubmeshIndex < mesh.subMeshCount)
        {
            affectedIndices.AddRange(mesh.GetTriangles(selectedSubmeshIndex).Distinct());
        }
        else
        {
            affectedIndices.AddRange(Enumerable.Range(0, mesh.vertexCount));
        }

        // If the filter is on, further reduce the list of affected vertices
        if (useVertexColorFilter && mesh.colors.Length > 0)
        {
            Color[] vertexColors = mesh.colors;
            affectedIndices = affectedIndices.Where(index =>
                index < vertexColors.Length && AreColorsApproximatelyEqual(vertexColors[index], selectedVertexColor)
            ).ToList();
        }

        // Apply transformations only to the final list of affected vertices
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


    private void CaptureSelectionState()
    {
        selectionStartUVs = new Dictionary<int, Vector2>();
        if (selectedUVIsslandIndices.Count == 0)
        {
            selectionCenter = Vector2.zero;
            return;
        }

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        foreach (int index in selectedUVIsslandIndices)
        {
            selectionStartUVs[index] = workingUvs[index];
            min.x = Mathf.Min(min.x, workingUvs[index].x);
            min.y = Mathf.Min(min.y, workingUvs[index].y);
            max.x = Mathf.Max(max.x, workingUvs[index].x);
            max.y = Mathf.Max(max.y, workingUvs[index].y);
        }
        selectionCenter = (min + max) / 2f;

        islandTransformOffset = Vector2.zero;
        islandTransformScale = Vector2.one;
    }

    private void ApplyIslandTransforms()
    {
        if (selectionStartUVs == null) return;

        foreach (var kvp in selectionStartUVs)
        {
            int index = kvp.Key;
            Vector2 startUv = kvp.Value;

            Vector2 pivotedUv = startUv - selectionCenter;
            pivotedUv.x *= islandTransformScale.x;
            pivotedUv.y *= islandTransformScale.y;
            Vector2 transformedUv = pivotedUv + selectionCenter + islandTransformOffset;

            workingUvs[index] = transformedUv;
        }
        mesh.SetUVs(selectedUVChannel, workingUvs);
        EditorUtility.SetDirty(mesh);
        Repaint();
    }

    private void CommitIslandTransforms()
    {
        ApplyIslandTransforms();
        CaptureSelectionState();
    }

    private void ResetIslandTransforms()
    {
        if (selectionStartUVs == null) return;
        foreach (var kvp in selectionStartUVs)
        {
            workingUvs[kvp.Key] = kvp.Value;
        }
        mesh.SetUVs(selectedUVChannel, workingUvs);
        EditorUtility.SetDirty(mesh);

        islandTransformOffset = Vector2.zero;
        islandTransformScale = Vector2.one;
        Repaint();
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

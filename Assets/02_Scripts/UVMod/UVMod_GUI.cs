using UnityEditor;
using UnityEngine;

public static class UVMod_GUI
{
    public static void DrawSettingsGUI(UVModWindow window, UVMod_Data data)
    {
        GUILayout.Label("UV Editor Window", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Select a 3D model. Use Scroll Wheel to Zoom. Use Scrollbars or Middle-Mouse-Drag to Pan.", MessageType.Info);
        EditorGUILayout.ObjectField("Selected Object", data.SelectedGameObject, typeof(GameObject), true);

        if (data.SelectedGameObject == null || data.Mesh == null) return;

        // --- Global Controls ---
        if (data.SelectedUVIsslandIndices.Count == 0)
        {
            GUI.enabled = data.SelectedUVIsslandIndices.Count == 0;
            EditorGUILayout.Space();
            GUILayout.Label("Global UV Modification", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            data.UvOffset = EditorGUILayout.Vector2Field("UV Offset", data.UvOffset);
            data.UvScale = EditorGUILayout.Vector2Field("UV Scale", data.UvScale);
            if (EditorGUI.EndChangeCheck())
            {
                UVMod_Actions.ApplyGlobalUVChanges(data);
            }
            GUI.enabled = true;
        }
        // --- Per-Island Controls ---
        else
        {
            EditorGUILayout.Space();
            GUI.color = Color.cyan;
            GUILayout.Label("Islands UV Modification", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            data.IslandTransformOffset = EditorGUILayout.Vector2Field("UV Offset", data.IslandTransformOffset);
            data.IslandTransformScale = EditorGUILayout.Vector2Field("UV Scale", data.IslandTransformScale);
            GUI.color = Color.white;
            if (EditorGUI.EndChangeCheck())
            {
                UVMod_Actions.ApplyIslandTransforms(data);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply")) UVMod_Actions.CommitIslandTransforms(data);
            if (GUILayout.Button("Reset")) UVMod_Actions.ResetIslandTransforms(data);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        GUILayout.Label("Settings & Tools", EditorStyles.boldLabel);

        data.SelectedUVChannel = EditorGUILayout.IntSlider("UV Channel", data.SelectedUVChannel, 0, 7);
        if (data.Mesh.subMeshCount > 1)
        {
            string[] submeshOptions = new string[data.Mesh.subMeshCount + 1];
            submeshOptions[0] = "All Submeshes";
            for (int i = 0; i < data.Mesh.subMeshCount; i++) submeshOptions[i + 1] = "Submesh " + i;
            data.SelectedSubmeshIndex = EditorGUILayout.Popup("Target Submesh", data.SelectedSubmeshIndex + 1, submeshOptions) - 1;
        }

        data.UseVertexColorFilter = EditorGUILayout.Toggle("Filter by Vertex Color", data.UseVertexColorFilter);
        if (data.UseVertexColorFilter)
        {
            EditorGUILayout.BeginHorizontal();
            {
                data.SelectedVertexColor = EditorGUILayout.ColorField("Target Vertex Color", data.SelectedVertexColor);
                if (data.SelectedUVIsslandIndices.Count > 0)
                {
                    GUI.enabled = data.UvTexturePreview != null;
                    if (GUILayout.Button("Pick & Apply Color", GUILayout.Width(140)))
                    {
                        data.IsPickingColor = true;
                    }
                    GUI.enabled = true;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Reset All UVs")) data.ResetUVs();
        if (GUILayout.Button("Save Modified Mesh in Folder"))
        {
            if(UVMod_Actions.SaveMeshAsset(data))
                data.LoadUVsFromChannel(data.SelectedUVChannel, true);
        }
    }

    public static void DrawUvEditorArea(UVModWindow window, UVMod_Data data)
    {
        Rect viewRect = GUILayoutUtility.GetRect(100, 100, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        Rect contentRect = new Rect(0, 0, viewRect.width * data.Zoom, viewRect.height * data.Zoom);

        if (data.IsPickingColor)
        {
            EditorGUI.DrawRect(viewRect, new Color(0, 1, 1, 0.2f));
            EditorGUIUtility.AddCursorRect(viewRect, MouseCursor.Arrow);
        }
        else
        {
            GUI.Box(viewRect, GUIContent.none);
        }

        UVMod_Events.HandleEvents(window, data, viewRect, contentRect);

        data.ScrollPosition = GUI.BeginScrollView(viewRect, data.ScrollPosition, contentRect, true, true);
        {
            Rect textureDisplayRect = UVMod_Actions.CalculateAspectRatioRect(contentRect, data.UvTexturePreview);

            if (data.UvTexturePreview != null)
            {
                GUI.DrawTexture(textureDisplayRect, data.UvTexturePreview, ScaleMode.ScaleToFit);
            }

            if (data.WorkingUvs != null && data.WorkingUvs.Length > 0)
            {
                // Draw Triangles
                for (int i = 0; i < data.Mesh.subMeshCount; i++)
                {
                    int[] triangles = data.Mesh.GetTriangles(i);
                    for (int j = 0; j < triangles.Length; j += 3)
                    {
                        int v1 = triangles[j], v2 = triangles[j + 1], v3 = triangles[j + 2];
                        if (v1 < data.WorkingUvs.Length && v2 < data.WorkingUvs.Length && v3 < data.WorkingUvs.Length)
                        {
                            Vector3 p1 = UVMod_Actions.ConvertUVToContentPos(data.WorkingUvs[v1], textureDisplayRect);
                            Vector3 p2 = UVMod_Actions.ConvertUVToContentPos(data.WorkingUvs[v2], textureDisplayRect);
                            Vector3 p3 = UVMod_Actions.ConvertUVToContentPos(data.WorkingUvs[v3], textureDisplayRect);

                            Handles.color = data.SelectedUVIsslandIndices.Contains(v1) ? Color.yellow : new Color(1, 0.3f, 0.3f, 0.8f);
                            Handles.DrawLine(p1, p2);
                            Handles.DrawLine(p2, p3);
                            Handles.DrawLine(p3, p1);
                        }
                    }
                }

                // Draw Vertices
                for (int index = 0; index < data.WorkingUvs.Length; index++)
                {
                    Vector3 handlePos = UVMod_Actions.ConvertUVToContentPos(data.WorkingUvs[index], textureDisplayRect);
                    Color handleColor;
                    bool isSelected = data.SelectedUVIsslandIndices.Contains(index);

                    if (isSelected)
                    {
                        handleColor = Color.cyan;
                    }
                    else if (data.Mesh.colors.Length == data.Mesh.vertexCount && data.Mesh.colors[index].a > 0)
                    {
                        handleColor = data.Mesh.colors[index];
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

        if (data.IsDraggingSelectionRect)
        {
            Handles.BeginGUI();
            Handles.color = Color.white;
            Handles.DrawSolidRectangleWithOutline(data.SelectionRect, new Color(0, 1, 1, 0.2f), Color.cyan);
            Handles.EndGUI();
        }
    }
}

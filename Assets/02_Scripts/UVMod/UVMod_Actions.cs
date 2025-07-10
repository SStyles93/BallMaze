using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class UVMod_Actions
{
    #region Coordinate Conversion and Color Picking
    public static void PickAndApplyVertexColor(UVMod_Data data, Vector2 screenPos, Rect viewRect, Rect contentRect)
    {
        if (data.UvTexturePreview == null || data.SelectedUVIsslandIndices.Count == 0) return;

        if (!data.UvTexturePreview.isReadable)
        {
            EditorUtility.DisplayDialog("Texture Not Readable", "The texture '" + data.UvTexturePreview.name + "' is not marked as readable. Please enable 'Read/Write Enabled' in its import settings.", "OK");
            return;
        }

        Rect textureDisplayRect = CalculateAspectRatioRect(contentRect, data.UvTexturePreview);
        Vector2 pickedUV = ConvertScreenPosToUVPos(data, screenPos, viewRect, textureDisplayRect);

        pickedUV.x = Mathf.Clamp01(pickedUV.x);
        pickedUV.y = Mathf.Clamp01(pickedUV.y);

        data.SelectedVertexColor = data.UvTexturePreview.GetPixelBilinear(pickedUV.x, pickedUV.y);

        Color[] vertexColors = data.Mesh.colors;
        if (vertexColors.Length != data.Mesh.vertexCount)
        {
            vertexColors = new Color[data.Mesh.vertexCount];
            for (int i = 0; i < vertexColors.Length; i++) vertexColors[i] = Color.white;
        }

        Undo.RecordObject(data.Mesh, "Apply Vertex Color");

        foreach (int vertIndex in data.SelectedUVIsslandIndices)
        {
            if (vertIndex < vertexColors.Length)
            {
                vertexColors[vertIndex] = data.SelectedVertexColor;
            }
        }

        data.Mesh.colors = vertexColors;
        EditorUtility.SetDirty(data.Mesh);
    }

    public static Rect CalculateAspectRatioRect(Rect container, Texture2D texture)
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

    public static Vector2 ConvertScreenPosToUVPos(UVMod_Data data, Vector2 screenPos, Rect viewRect, Rect textureDisplayRect)
    {
        Vector2 localPos = screenPos - viewRect.position;
        Vector2 posInContent = localPos + data.ScrollPosition;
        Vector2 posInTexture = posInContent - textureDisplayRect.position;
        return new Vector2(posInTexture.x / textureDisplayRect.width, 1 - posInTexture.y / textureDisplayRect.height);
    }

    public static Vector3 ConvertUVToContentPos(Vector2 uvPos, Rect textureDisplayRect)
    {
        float x = textureDisplayRect.x + uvPos.x * textureDisplayRect.width;
        float y = textureDisplayRect.y + (1 - uvPos.y) * textureDisplayRect.height;
        return new Vector3(x, y, 0);
    }

    public static Vector3 ConvertUVToScreenPos(UVMod_Data data, Vector2 uvPos, Rect viewRect, Rect textureDisplayRect)
    {
        Vector2 posInContent = new Vector2(
            textureDisplayRect.x + uvPos.x * textureDisplayRect.width,
            textureDisplayRect.y + (1 - uvPos.y) * textureDisplayRect.height
        );
        Vector2 screenPos = posInContent - data.ScrollPosition + viewRect.position;
        return new Vector3(screenPos.x, screenPos.y, 0);
    }
    #endregion

    #region Data Modification and Saving
    public static void ApplyGlobalUVChanges(UVMod_Data data)
    {
        if (data.Mesh == null || data.InitialUvs == null) return;

        data.WorkingUvs = (Vector2[])data.InitialUvs.Clone();

        List<int> affectedIndices = new List<int>();
        if (data.SelectedSubmeshIndex >= 0 && data.SelectedSubmeshIndex < data.Mesh.subMeshCount)
        {
            affectedIndices.AddRange(data.Mesh.GetTriangles(data.SelectedSubmeshIndex).Distinct());
        }
        else
        {
            affectedIndices.AddRange(Enumerable.Range(0, data.Mesh.vertexCount));
        }

        if (data.UseVertexColorFilter && data.Mesh.colors.Length > 0)
        {
            Color[] vertexColors = data.Mesh.colors;
            affectedIndices = affectedIndices.Where(index =>
                index < vertexColors.Length && AreColorsApproximatelyEqual(vertexColors[index], data.SelectedVertexColor)
            ).ToList();
        }

        foreach (int index in affectedIndices)
        {
            if (index < data.WorkingUvs.Length)
            {
                data.WorkingUvs[index] = new Vector2(data.InitialUvs[index].x * data.UvScale.x + data.UvOffset.x, data.InitialUvs[index].y * data.UvScale.y + data.UvOffset.y);
            }
        }

        data.Mesh.SetUVs(data.SelectedUVChannel, data.WorkingUvs);
        EditorUtility.SetDirty(data.Mesh);
    }

    public static void CaptureSelectionState(UVMod_Data data)
    {
        data.SelectionStartUVs = new Dictionary<int, Vector2>();
        if (data.SelectedUVIsslandIndices.Count == 0)
        {
            data.SelectionCenter = Vector2.zero;
            return;
        }

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        foreach (int index in data.SelectedUVIsslandIndices)
        {
            data.SelectionStartUVs[index] = data.WorkingUvs[index];
            min.x = Mathf.Min(min.x, data.WorkingUvs[index].x);
            min.y = Mathf.Min(min.y, data.WorkingUvs[index].y);
            max.x = Mathf.Max(max.x, data.WorkingUvs[index].x);
            max.y = Mathf.Max(max.y, data.WorkingUvs[index].y);
        }
        data.SelectionCenter = (min + max) / 2f;

        data.IslandTransformOffset = Vector2.zero;
        data.IslandTransformScale = Vector2.one;
    }

    public static void ApplyIslandTransforms(UVMod_Data data)
    {
        if (data.SelectionStartUVs == null) return;

        foreach (var kvp in data.SelectionStartUVs)
        {
            int index = kvp.Key;
            Vector2 startUv = kvp.Value;

            Vector2 pivotedUv = startUv - data.SelectionCenter;
            pivotedUv.x *= data.IslandTransformScale.x;
            pivotedUv.y *= data.IslandTransformScale.y;
            Vector2 transformedUv = pivotedUv + data.SelectionCenter + data.IslandTransformOffset;

            data.WorkingUvs[index] = transformedUv;
        }
        data.Mesh.SetUVs(data.SelectedUVChannel, data.WorkingUvs);
        EditorUtility.SetDirty(data.Mesh);
    }

    public static void CommitIslandTransforms(UVMod_Data data)
    {
        ApplyIslandTransforms(data);
        CaptureSelectionState(data);
    }

    public static void ResetIslandTransforms(UVMod_Data data)
    {
        if (data.SelectionStartUVs == null) return;
        foreach (var kvp in data.SelectionStartUVs)
        {
            data.WorkingUvs[kvp.Key] = kvp.Value;
        }
        data.Mesh.SetUVs(data.SelectedUVChannel, data.WorkingUvs);
        EditorUtility.SetDirty(data.Mesh);

        data.IslandTransformOffset = Vector2.zero;
        data.IslandTransformScale = Vector2.one;
    }

    public static void SaveMeshAsset(UVMod_Data data)
    {
        if (data.Mesh == null) return;
        string path = AssetDatabase.GetAssetPath(data.Mesh);
        if (string.IsNullOrEmpty(path) || !AssetDatabase.IsMainAsset(data.Mesh))
        {
            path = EditorUtility.SaveFilePanelInProject("Save Modified Mesh", data.SelectedGameObject.name + "_Modified", "asset", "Save the modified mesh.");
            if (string.IsNullOrEmpty(path)) return;

            Mesh newMesh = Object.Instantiate(data.Mesh);
            newMesh.name = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(newMesh, path);
            data.MeshFilter.mesh = newMesh;
        }

        data.Mesh.SetUVs(data.SelectedUVChannel, data.WorkingUvs);
        EditorUtility.SetDirty(data.Mesh);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Success", "Mesh asset saved/updated at: " + path, "OK");
    }

    public static bool AreColorsApproximatelyEqual(Color c1, Color c2, float tolerance = 0.01f)
    {
        return Mathf.Abs(c1.r - c2.r) < tolerance &&
               Mathf.Abs(c1.g - c2.g) < tolerance &&
               Mathf.Abs(c1.b - c2.b) < tolerance &&
               Mathf.Abs(c1.a - c2.a) < tolerance;
    }
    #endregion
}

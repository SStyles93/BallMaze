using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class UVMod_Events
{
    public static void HandleEvents(UVModWindow window, UVMod_Data data, Rect viewRect, Rect contentRect)
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
                float oldZoom = data.Zoom;
                float zoomDelta = -currentEvent.delta.y * 0.05f;
                data.Zoom = Mathf.Clamp(data.Zoom + zoomDelta * data.Zoom, UVMod_Data.MinZoom, UVMod_Data.MaxZoom);

                Vector2 mousePosInView = currentEvent.mousePosition - viewRect.position;
                data.ScrollPosition = ((data.ScrollPosition + mousePosInView) * (data.Zoom / oldZoom)) - mousePosInView;

                window.Repaint();
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
                    data.IsPanning = true;
                    data.PanStartMousePos = currentEvent.mousePosition;
                    GUIUtility.hotControl = controlID;
                    currentEvent.Use();
                    return;
                }

                if (currentEvent.button == 0)
                {
                    if (data.IsPickingColor)
                    {
                        UVMod_Actions.PickAndApplyVertexColor(data, currentEvent.mousePosition, viewRect, contentRect);
                        data.IsPickingColor = false;
                        GUIUtility.hotControl = controlID;
                        currentEvent.Use();
                        return;
                    }

                    GUIUtility.hotControl = controlID;
                    data.ActiveUVHandle = -1;
                    Rect textureDisplayRect = UVMod_Actions.CalculateAspectRatioRect(contentRect, data.UvTexturePreview);
                    Vector2 mouseUVPos = UVMod_Actions.ConvertScreenPosToUVPos(data, currentEvent.mousePosition, viewRect, textureDisplayRect);
                    float minDistance = (0.02f / data.Zoom);

                    int topHandle = -1;
                    float minD = float.MaxValue;
                    for (int i = 0; i < data.WorkingUvs.Length; i++)
                    {
                        float d = Vector2.Distance(data.WorkingUvs[i], mouseUVPos);
                        if (d < minDistance && d < minD)
                        {
                            minD = d;
                            topHandle = i;
                        }
                    }

                    if (topHandle != -1)
                    {
                        data.ActiveUVHandle = topHandle;
                        data.DragStartMousePos = currentEvent.mousePosition;
                        Undo.RecordObject(data.Mesh, "Drag UV Point");

                        if (!data.SelectedUVIsslandIndices.Contains(data.ActiveUVHandle))
                        {
                            if (!currentEvent.shift)
                            {
                                data.SelectedUVIsslandIndices.Clear();
                            }
                            foreach (var island in data.UvIslands)
                            {
                                if (island.Contains(data.ActiveUVHandle))
                                {
                                    data.SelectedUVIsslandIndices.AddRange(island.Except(data.SelectedUVIsslandIndices));
                                    break;
                                }
                            }
                            UVMod_Actions.CaptureSelectionState(data);
                        }

                        data.DragStartIslandUVs = new Dictionary<int, Vector2>();
                        foreach (int index in data.SelectedUVIsslandIndices)
                        {
                            data.DragStartIslandUVs[index] = data.WorkingUvs[index];
                        }
                    }
                    else
                    {
                        data.IsDraggingSelectionRect = true;
                        data.SelectionRectStartPos = currentEvent.mousePosition;
                        data.SelectionRect = new Rect(data.SelectionRectStartPos.x, data.SelectionRectStartPos.y, 0, 0);
                    }
                    currentEvent.Use();
                }
            }
        }

        if (eventType == EventType.MouseDrag && GUIUtility.hotControl == controlID)
        {
            if (data.IsPanning)
            {
                Vector2 delta = currentEvent.mousePosition - data.PanStartMousePos;
                data.ScrollPosition -= delta;
                data.PanStartMousePos = currentEvent.mousePosition;
            }
            else if (data.ActiveUVHandle != -1)
            {
                Rect textureDisplayRect = UVMod_Actions.CalculateAspectRatioRect(contentRect, data.UvTexturePreview);
                Vector2 startMouseUV = UVMod_Actions.ConvertScreenPosToUVPos(data, data.DragStartMousePos, viewRect, textureDisplayRect);
                Vector2 currentMouseUV = UVMod_Actions.ConvertScreenPosToUVPos(data, currentEvent.mousePosition, viewRect, textureDisplayRect);
                Vector2 deltaUV = currentMouseUV - startMouseUV;

                foreach (var islandKvp in data.DragStartIslandUVs)
                {
                    data.WorkingUvs[islandKvp.Key] = islandKvp.Value + deltaUV;
                }
                UVMod_Actions.CaptureSelectionState(data);
            }
            else if (data.IsDraggingSelectionRect)
            {
                data.SelectionRect = new Rect(
                    Mathf.Min(data.SelectionRectStartPos.x, currentEvent.mousePosition.x),
                    Mathf.Min(data.SelectionRectStartPos.y, currentEvent.mousePosition.y),
                    Mathf.Abs(data.SelectionRectStartPos.x - currentEvent.mousePosition.x),
                    Mathf.Abs(data.SelectionRectStartPos.y - currentEvent.mousePosition.y)
                );
            }
            window.Repaint();
            currentEvent.Use();
        }

        if (eventType == EventType.MouseUp && GUIUtility.hotControl == controlID)
        {
            GUIUtility.hotControl = 0;
            data.IsPanning = false;

            if (data.IsDraggingSelectionRect)
            {
                data.IsDraggingSelectionRect = false;
                if (!currentEvent.shift) data.SelectedUVIsslandIndices.Clear();

                Rect textureDisplayRect = UVMod_Actions.CalculateAspectRatioRect(contentRect, data.UvTexturePreview);
                foreach (var island in data.UvIslands)
                {
                    bool islandIntersects = island.Any(uvIndex => data.SelectionRect.Contains(UVMod_Actions.ConvertUVToScreenPos(data, data.WorkingUvs[uvIndex], viewRect, textureDisplayRect)));
                    if (islandIntersects)
                    {
                        data.SelectedUVIsslandIndices.AddRange(island.Except(data.SelectedUVIsslandIndices));
                    }
                }
                UVMod_Actions.CaptureSelectionState(data);
            }
            else if (data.ActiveUVHandle != -1)
            {
                data.Mesh.SetUVs(data.SelectedUVChannel, new List<Vector2>(data.WorkingUvs));
                EditorUtility.SetDirty(data.Mesh);
                UVMod_Actions.CaptureSelectionState(data);
            }

            data.ActiveUVHandle = -1;
            data.DragStartIslandUVs = null;
            window.Repaint();
            currentEvent.Use();
        }
    }
}

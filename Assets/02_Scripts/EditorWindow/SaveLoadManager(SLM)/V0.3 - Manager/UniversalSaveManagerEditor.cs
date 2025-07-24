using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(UniversalSaveManager))]
public class UniversalSaveManagerEditor : Editor
{
    private UniversalSaveManager manager;

    private void OnEnable()
    {
        manager = (UniversalSaveManager)target;
    }

    public override void OnInspectorGUI()
    {
        // Draw the default inspector fields first
        DrawDefaultInspector();

        EditorGUILayout.Space();
        GUILayout.Label("Drag GameObjects to Save:", EditorStyles.boldLabel);

        // Drag and drop area
        Rect dropArea = EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Height(50));
        GUI.Box(dropArea, "Drag GameObjects Here");
        EditorGUILayout.EndVertical();

        Event evt = Event.current;
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition)) break;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is GameObject && !manager.gameObjectsToSave.Contains((GameObject)draggedObject))
                        {
                            manager.gameObjectsToSave.Add((GameObject)draggedObject);
                            EditorUtility.SetDirty(manager); // Mark manager as dirty to save changes
                        }
                    }
                }
                Event.current.Use();
                break;
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Save All GameObjects"))
        {
            manager.SaveAllGameObjects();
        }

        if (GUILayout.Button("Load All GameObjects"))
        {
            manager.LoadAllGameObjects();
        }
    }
}



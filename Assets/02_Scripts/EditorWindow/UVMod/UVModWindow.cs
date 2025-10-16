using UnityEditor;
using UnityEngine;

namespace PxP.Tools
{
    public class UVModWindow : EditorWindow
    {
        public UVMod_Data Data { get; private set; }
        public SerializedObject SerializedData { get; private set; }

        [MenuItem("PxPTools/UV Editor")]
        public static void ShowWindow()
        {
            GetWindow<UVModWindow>("UV Mod");
        }

        private void OnEnable()
        {
            if (Data == null)
            {
                Data = ScriptableObject.CreateInstance<UVMod_Data>();
                SerializedData = new SerializedObject(Data);
            }
            Selection.selectionChanged += OnSelectionChange;
            Undo.undoRedoPerformed += OnUndoRedo; // Repaint on Undo/Redo
            OnSelectionChange();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChange;
            Undo.undoRedoPerformed -= OnUndoRedo;

            if (Data != null)
            {
                DestroyImmediate(Data);
            }
        }

        private void OnSelectionChange()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject != null && (Data.SelectedGameObject != selectedObject))
            {
                Data.LoadDataFromSelection(selectedObject);
                SerializedData = new SerializedObject(Data); // Re-create SerializedObject for the new data
            }
            else if (selectedObject == null)
            {
                Data.LoadDataFromSelection(null);
                SerializedData = new SerializedObject(Data);
            }
            Repaint();
        }

        private void OnUndoRedo()
        {
            // When an undo or redo is performed, re-read the data from the serialized object
            // and apply changes if necessary.
            if (SerializedData != null)
            {
                SerializedData.Update();
                if (Data.Mesh != null)
                {
                    UVMod_Actions.ApplyIslandTransforms(Data);
                    UVMod_Actions.ApplyGlobalUVChanges(Data);
                }
            }
            Repaint();
        }

        void OnGUI()
        {
            if (SerializedData == null) return;

            SerializedData.Update(); // Always start with updating the serialized object

            UVMod_GUI.DrawSettingsGUI(this, Data, SerializedData);
            EditorGUILayout.Space();

            GUILayout.Label("Visual UV Editor", EditorStyles.boldLabel);
            UVMod_GUI.DrawUvEditorArea(this, Data);

            // Apply changes at the end of the GUI loop
            SerializedData.ApplyModifiedProperties();
        }
    }
}

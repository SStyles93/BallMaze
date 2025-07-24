using UnityEditor;
using UnityEngine;


namespace PxP.Tools
{
    public class UVModWindow : EditorWindow
    {
        // The central data container for the entire tool.
        public UVMod_Data Data { get; private set; }

        [MenuItem("PxPTools/UV Editor")]
        public static void ShowWindow()
        {
            GetWindow<UVModWindow>("UV Mod");
        }

        private void OnEnable()
        {
            // Initialize the data container when the window is enabled.
            Data = new UVMod_Data();
            Selection.selectionChanged += OnSelectionChange;
            OnSelectionChange(); // Initial setup
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChange;
        }

        /// <summary>
        /// Handles selection changes by delegating to the data class.
        /// </summary>
        private void OnSelectionChange()
        {
            Data.LoadDataFromSelection(Selection.activeGameObject);
            Repaint();
        }

        /// <summary>
        /// The main OnGUI loop, which delegates drawing to the specialized GUI class.
        /// </summary>
        void OnGUI()
        {
            UVMod_GUI.DrawSettingsGUI(this, Data);

            EditorGUILayout.Space();
            GUILayout.Label("Visual UV Editor", EditorStyles.boldLabel);

            UVMod_GUI.DrawUvEditorArea(this, Data);
        }
    }
}
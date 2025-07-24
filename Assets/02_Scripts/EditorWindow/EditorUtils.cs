using UnityEngine;
using UnityEditor;

/// <summary>
/// A utility class for creating common editor controls that automatically handle
/// Undo/Redo, marking the object as dirty, and repainting the inspector.
/// </summary>
public static class EditorUtils
{
    #region Vector Fields

    /// <summary>
    /// Displays a Vector2 field that records changes for Undo/Redo.
    /// </summary>
    /// <param name="label">The label to display next to the field.</param>
    /// <param name="description">A tooltip description for the field.</param>
    /// <param name="currentValue">The current value of the property.</param>
    /// <param name="targetObject">The object being inspected (e.g., the MonoBehaviour or ScriptableObject).</param>
    /// <param name="editorWindow">The editor window that needs to be repainted. Can be null if not needed.</param>
    /// <returns>The potentially changed value of the property.</returns>
    public static Vector2 RecordVector2Field(string label, string description, Vector2 currentValue, Object targetObject, Editor editorWindow = null)
    {
        EditorGUI.BeginChangeCheck();
        Vector2 newValue = EditorGUILayout.Vector2Field(new GUIContent(label, description), currentValue);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetObject, $"{label} changed");
            EditorUtility.SetDirty(targetObject);
            if (editorWindow != null) editorWindow.Repaint();
            return newValue;
        }
        return currentValue;
    }

    /// <summary>
    /// Displays a Vector2Int field that records changes for Undo/Redo.
    /// </summary>
    /// <param name="label">The label to display next to the field.</param>
    /// <param name="description">A tooltip description for the field.</param>
    /// <param name="currentValue">The current value of the property.</param>
    /// <param name="targetObject">The object being inspected.</param>
    /// <param name="editorWindow">The editor window that needs to be repainted. Can be null if not needed.</param>
    /// <returns>The potentially changed value of the property.</returns>
    public static Vector2Int RecordVector2IntField(string label, string description, Vector2Int currentValue, Object targetObject, Editor editorWindow = null)
    {
        EditorGUI.BeginChangeCheck();
        Vector2Int newValue = EditorGUILayout.Vector2IntField(new GUIContent(label, description), currentValue);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetObject, $"{label} changed");
            EditorUtility.SetDirty(targetObject);
            if (editorWindow != null) editorWindow.Repaint();
            return newValue;
        }
        return currentValue;
    }

    /// <summary>
    /// Displays a Vector3 field that records changes for Undo/Redo.
    /// </summary>
    /// <param name="label">The label to display next to the field.</param>
    /// <param name="description">A tooltip description for the field.</param>
    /// <param name="currentValue">The current value of the property.</param>
    /// <param name="targetObject">The object being inspected.</param>
    /// <param name="editorWindow">The editor window that needs to be repainted. Can be null if not needed.</param>
    /// <returns>The potentially changed value of the property.</returns>
    public static Vector3 RecordVector3Field(string label, string description, Vector3 currentValue, Object targetObject, Editor editorWindow = null)
    {
        EditorGUI.BeginChangeCheck();
        Vector3 newValue = EditorGUILayout.Vector3Field(new GUIContent(label, description), currentValue);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetObject, $"{label} changed");
            EditorUtility.SetDirty(targetObject);
            if (editorWindow != null) editorWindow.Repaint();
            return newValue;
        }
        return currentValue;
    }

    /// <summary>
    /// Displays a Vector3Int field that records changes for Undo/Redo.
    /// </summary>
    /// <param name="label">The label to display next to the field.</param>
    /// <param name="description">A tooltip description for the field.</param>
    /// <param name="currentValue">The current value of the property.</param>
    /// <param name="targetObject">The object being inspected.</param>
    /// <param name="editorWindow">The editor window that needs to be repainted. Can be null if not needed.</param>
    /// <returns>The potentially changed value of the property.</returns>
    public static Vector3Int RecordVector3IntField(string label, string description, Vector3Int currentValue, Object targetObject, Editor editorWindow = null)
    {
        EditorGUI.BeginChangeCheck();
        Vector3Int newValue = EditorGUILayout.Vector3IntField(new GUIContent(label, description), currentValue);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetObject, $"{label} changed");
            EditorUtility.SetDirty(targetObject);
            if (editorWindow != null) editorWindow.Repaint();
            return newValue;
        }
        return currentValue;
    }

    #endregion

    #region Enum Field

    /// <summary>
    /// Displays an enum popup that records changes for Undo/Redo.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="label">The label to display next to the field.</param>
    /// <param name="description">A tooltip description for the field.</param>
    /// <param name="currentValue">The current value of the property.</param>
    /// <param name="targetObject">The object being inspected.</param>
    /// <param name="editorWindow">The editor window that needs to be repainted. Can be null if not needed.</param>
    /// <returns>The potentially changed value of the property.</returns>
    public static T RecordEnum<T>(string label, string description, T currentValue, Object targetObject, Editor editorWindow = null) where T : System.Enum
    {
        EditorGUI.BeginChangeCheck();
        var newValue = (T)EditorGUILayout.EnumPopup(new GUIContent(label, description), currentValue);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetObject, $"{label} changed to {currentValue.ToString()}");
            EditorUtility.SetDirty(targetObject);
            if (editorWindow != null) editorWindow.Repaint();
            return newValue;
        }
        return currentValue;
    }

    #endregion

    #region Text and Number Fields

    /// <summary>
    /// Displays a text field that records changes for Undo/Redo.
    /// </summary>
    /// <param name="label">The label to display next to the field.</param>
    /// <param name="description">A tooltip description for the field.</param>
    /// <param name="currentValue">The current value of the property.</param>
    /// <param name="targetObject">The object being inspected.</param>
    /// <param name="editorWindow">The editor window that needs to be repainted. Can be null if not needed.</param>
    /// <returns>The potentially changed value of the property.</returns>
    public static string RecordTextField(string label, string description, string currentValue, Object targetObject, Editor editorWindow = null)
    {
        EditorGUI.BeginChangeCheck();
        string newValue = EditorGUILayout.TextField(new GUIContent(label, description), currentValue);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetObject, $"{label} changed");
            EditorUtility.SetDirty(targetObject);
            if (editorWindow != null) editorWindow.Repaint();
            return newValue;
        }
        return currentValue;
    }

    /// <summary>
    /// Displays a float field that records changes for Undo/Redo.
    /// </summary>
    /// <param name="label">The label to display next to the field.</param>
    /// <param name="description">A tooltip description for the field.</param>
    /// <param name="currentValue">The current value of the property.</param>
    /// <param name="targetObject">The object being inspected.</param>
    /// <param name="editorWindow">The editor window that needs to be repainted. Can be null if not needed.</param>
    /// <returns>The potentially changed value of the property.</returns>
    public static float RecordFloatField(string label, string description, float currentValue, Object targetObject, Editor editorWindow = null)
    {
        EditorGUI.BeginChangeCheck();
        float newValue = EditorGUILayout.FloatField(new GUIContent(label, description), currentValue);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetObject, $"{label} changed");
            EditorUtility.SetDirty(targetObject);
            if (editorWindow != null) editorWindow.Repaint();
            return newValue;
        }
        return currentValue;
    }

    /// <summary>
    /// Displays an integer field that records changes for Undo/Redo.
    /// </summary>
    /// <param name="label">The label to display next to the field.</param>
    /// <param name="description">A tooltip description for the field.</param>
    /// <param name="currentValue">The current value of the property.</param>
    /// <param name="targetObject">The object being inspected.</param>
    /// <param name="editorWindow">The editor window that needs to be repainted. Can be null if not needed.</param>
    /// <returns>The potentially changed value of the property.</returns>
    public static int RecordIntField(string label, string description, int currentValue, Object targetObject, Editor editorWindow = null)
    {
        EditorGUI.BeginChangeCheck();
        int newValue = EditorGUILayout.IntField(new GUIContent(label, description), currentValue);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetObject, $"{label} changed");
            EditorUtility.SetDirty(targetObject);
            if (editorWindow != null) editorWindow.Repaint();
            return newValue;
        }
        return currentValue;
    }

    #endregion

    #region Toggle and Sliders

    /// <summary>
    /// Displays a toggle (checkbox) that records changes for Undo/Redo.
    /// </summary>
    /// <param name="label">The label to display next to the field.</param>
    /// <param name="description">A tooltip description for the field.</param>
    /// <param name="currentValue">The current value of the property.</param>
    /// <param name="targetObject">The object being inspected.</param>
    /// <param name="editorWindow">The editor window that needs to be repainted. Can be null if not needed.</param>
    /// <returns>The potentially changed value of the property.</returns>
    public static bool RecordFoldout(bool currentValue, string label, string description, Object targetObject, Editor editorWindow = null)
    {
        EditorGUI.BeginChangeCheck();
        bool newValue = EditorGUILayout.Foldout(currentValue, new GUIContent(label, description));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetObject, $"{label} changed");
            EditorUtility.SetDirty(targetObject);
            if (editorWindow != null) editorWindow.Repaint();
            return newValue;
        }
        return currentValue;
    }

    /// <summary>
    /// Displays a toggle (checkbox) that records changes for Undo/Redo.
    /// </summary>
    /// <param name="label">The label to display next to the field.</param>
    /// <param name="description">A tooltip description for the field.</param>
    /// <param name="currentValue">The current value of the property.</param>
    /// <param name="targetObject">The object being inspected.</param>
    /// <param name="editorWindow">The editor window that needs to be repainted. Can be null if not needed.</param>
    /// <returns>The potentially changed value of the property.</returns>
    public static bool RecordToggle(string label, string description, bool currentValue, Object targetObject, Editor editorWindow = null)
    {
        EditorGUI.BeginChangeCheck();
        bool newValue = EditorGUILayout.Toggle(new GUIContent(label, description), currentValue);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetObject, $"{label} changed");
            EditorUtility.SetDirty(targetObject);
            if (editorWindow != null) editorWindow.Repaint();
            return newValue;
        }
        return currentValue;
    }

    /// <summary>
    /// Displays a toggle (checkbox) that records changes for Undo/Redo.
    /// </summary>
    /// <param name="label">The label to display next to the field.</param>
    /// <param name="description">A tooltip description for the field.</param>
    /// <param name="currentValue">The current value of the property.</param>
    /// <param name="targetObject">The object being inspected.</param>
    /// <param name="editorWindow">The editor window that needs to be repainted. Can be null if not needed.</param>
    /// <returns>The potentially changed value of the property.</returns>
    public static bool RecordToggleLeft(string label, bool currentValue, Object targetObject, Editor editorWindow = null, params GUILayoutOption[] options)
    {
        EditorGUI.BeginChangeCheck();
        bool newValue = EditorGUILayout.ToggleLeft(new GUIContent(label), currentValue, options);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetObject, $"{label} changed");
            EditorUtility.SetDirty(targetObject);
            if (editorWindow != null) editorWindow.Repaint();
            return newValue;
        }
        return currentValue;
    }

    /// <summary>
    /// Displays a slider for floats that records changes for Undo/Redo.
    /// </summary>
    /// <param name="label">The label to display next to the field.</param>
    /// <param name="description">A tooltip description for the field.</param>
    /// <param name="currentValue">The current value of the property.</param>
    /// <param name="min">The minimum value of the slider.</param>
    /// <param name="max">The maximum value of the slider.</param>
    /// <param name="targetObject">The object being inspected.</param>
    /// <param name="editorWindow">The editor window that needs to be repainted. Can be null if not needed.</param>
    /// <returns>The potentially changed value of the property.</returns>
    public static float RecordSlider(string label, string description, float currentValue, float min, float max, Object targetObject, Editor editorWindow = null)
    {
        EditorGUI.BeginChangeCheck();
        float newValue = EditorGUILayout.Slider(new GUIContent(label, description), currentValue, min, max);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetObject, $"{label} changed");
            EditorUtility.SetDirty(targetObject);
            if (editorWindow != null) editorWindow.Repaint();
            return newValue;
        }
        return currentValue;
    }

    /// <summary>
    /// Displays a slider for integers that records changes for Undo/Redo.
    /// </summary>
    /// <param name="label">The label to display next to the field.</param>
    /// <param name="description">A tooltip description for the field.</param>
    /// <param name="currentValue">The current value of the property.</param>
    /// <param name="min">The minimum value of the slider.</param>
    /// <param name="max">The maximum value of the slider.</param>
    /// <param name="targetObject">The object being inspected.</param>
    /// <param name="editorWindow">The editor window that needs to be repainted. Can be null if not needed.</param>
    /// <returns>The potentially changed value of the property.</returns>
    public static int RecordIntSlider(string label, string description, int currentValue, int min, int max, Object targetObject, Editor editorWindow = null)
    {
        EditorGUI.BeginChangeCheck();
        int newValue = EditorGUILayout.IntSlider(new GUIContent(label, description), currentValue, min, max);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetObject, $"{label} changed");
            EditorUtility.SetDirty(targetObject);
            if (editorWindow != null) editorWindow.Repaint();
            return newValue;
        }
        return currentValue;
    }

    #endregion

    #region Object Field

    /// <summary>
    /// Displays an object field that records changes for Undo/Redo.
    /// </summary>
    /// <typeparam name="T">The type of the object (e.g., GameObject, Material, Transform).</typeparam>
    /// <param name="label">The label to display next to the field.</param>
    /// <param name="description">A tooltip description for the field.</param>
    /// <param name="currentValue">The current value of the property.</param>
    /// <param name="allowSceneObjects">Whether objects from the scene are allowed.</param>
    /// <param name="targetObject">The object being inspected.</param>
    /// <param name="editorWindow">The editor window that needs to be repainted. Can be null if not needed.</param>
    /// <returns>The potentially changed value of the property.</returns>
    public static T RecordObjectField<T>(string label, string description, T currentValue, bool allowSceneObjects, Object targetObject, Editor editorWindow = null) where T : Object
    {
        EditorGUI.BeginChangeCheck();
        T newValue = (T)EditorGUILayout.ObjectField(new GUIContent(label, description), currentValue, typeof(T), allowSceneObjects);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetObject, $"{label} changed");
            EditorUtility.SetDirty(targetObject);
            if (editorWindow != null) editorWindow.Repaint();
            return newValue;
        }
        return currentValue;
    }

    #endregion

    #region Other Common Fields

    /// <summary>
    /// Displays a color field that records changes for Undo/Redo.
    /// </summary>
    /// <param name="label">The label to display next to the field.</param>
    /// <param name="description">A tooltip description for the field.</param>
    /// <param name="currentValue">The current value of the property.</param>
    /// <param name="targetObject">The object being inspected.</param>
    /// <param name="editorWindow">The editor window that needs to be repainted. Can be null if not needed.</param>
    /// <returns>The potentially changed value of the property.</returns>
    public static Color RecordColorField(string label, string description, Color currentValue, Object targetObject, Editor editorWindow = null)
    {
        EditorGUI.BeginChangeCheck();
        Color newValue = EditorGUILayout.ColorField(new GUIContent(label, description), currentValue);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetObject, $"{label} changed");
            EditorUtility.SetDirty(targetObject);
            if (editorWindow != null) editorWindow.Repaint();
            return newValue;
        }
        return currentValue;
    }

    /// <summary>
    /// Displays a layer field that records changes for Undo/Redo.
    /// </summary>
    /// <param name="label">The label to display next to the field.</param>
    /// <param name="description">A tooltip description for the field.</param>
    /// <param name="currentValue">The current value of the property (the layer index).</param>
    /// <param name="targetObject">The object being inspected.</param>
    /// <param name="editorWindow">The editor window that needs to be repainted. Can be null if not needed.</param>
    /// <returns>The potentially changed value of the property.</returns>
    public static int RecordLayerField(string label, string description, int currentValue, Object targetObject, Editor editorWindow = null)
    {
        EditorGUI.BeginChangeCheck();
        int newValue = EditorGUILayout.LayerField(new GUIContent(label, description), currentValue);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetObject, $"{label} changed");
            EditorUtility.SetDirty(targetObject);
            if (editorWindow != null) editorWindow.Repaint();
            return newValue;
        }
        return currentValue;
    }

    #endregion
}

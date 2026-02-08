using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(ITutorialCondition), true)]
public class TutorialConditionDrawer : PropertyDrawer
{
    private static readonly List<Type> conditionTypes;
    private static readonly string[] displayNames;

    static TutorialConditionDrawer()
    {
        conditionTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch { return Array.Empty<Type>(); }
            })
            .Where(t =>
                typeof(ITutorialCondition).IsAssignableFrom(t) &&
                !t.IsInterface &&
                !t.IsAbstract)
            .ToList();

        displayNames = new string[conditionTypes.Count + 1];
        displayNames[0] = "None";

        for (int i = 0; i < conditionTypes.Count; i++)
        {
            displayNames[i + 1] =
                ObjectNames.NicifyVariableName(conditionTypes[i].Name);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.managedReferenceValue == null)
            return EditorGUIUtility.singleLineHeight;

        float height = EditorGUIUtility.singleLineHeight;

        SerializedProperty iterator = property.Copy();
        SerializedProperty end = iterator.GetEndProperty();

        iterator.NextVisible(true); // skip managed reference root

        while (!SerializedProperty.EqualContents(iterator, end))
        {
            height += EditorGUI.GetPropertyHeight(iterator, true) + 2;
            iterator.NextVisible(false);
        }

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Improve label for list usage
        if (property.propertyPath.Contains("Array.data"))
        {
            int index = GetArrayIndex(property.propertyPath);
            label = new GUIContent($"Condition {index + 1}");
        }

        // Dropdown
        Rect dropdownRect = new Rect(
            position.x,
            position.y,
            position.width,
            EditorGUIUtility.singleLineHeight);

        int currentIndex = 0;

        if (property.managedReferenceValue != null)
        {
            Type currentType = property.managedReferenceValue.GetType();
            currentIndex = conditionTypes.IndexOf(currentType) + 1;
        }

        int newIndex = EditorGUI.Popup(
            dropdownRect, label, currentIndex,
            displayNames.Select(n => new GUIContent(n)).ToArray());


        if (newIndex != currentIndex)
        {
            if (newIndex == 0)
            {
                property.managedReferenceValue = null;
            }
            else
            {
                property.managedReferenceValue =
                    Activator.CreateInstance(conditionTypes[newIndex - 1]);
            }
        }

        // Draw child fields
        if (property.managedReferenceValue != null)
        {
            Rect contentRect = new Rect(
                position.x,
                position.y + EditorGUIUtility.singleLineHeight + 2,
                position.width,
                position.height - EditorGUIUtility.singleLineHeight - 2);

            SerializedProperty iterator = property.Copy();
            SerializedProperty end = iterator.GetEndProperty();
            iterator.NextVisible(true);

            EditorGUI.indentLevel++;

            while (!SerializedProperty.EqualContents(iterator, end))
            {
                float h = EditorGUI.GetPropertyHeight(iterator, true);
                Rect r = new Rect(contentRect.x, contentRect.y, contentRect.width, h);
                EditorGUI.PropertyField(r, iterator, true);
                contentRect.y += h + 2;

                iterator.NextVisible(false);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private int GetArrayIndex(string propertyPath)
    {
        int start = propertyPath.LastIndexOf('[') + 1;
        int end = propertyPath.LastIndexOf(']');
        if (start >= 0 && end >= 0)
        {
            string indexStr = propertyPath.Substring(start, end - start);
            if (int.TryParse(indexStr, out int index))
                return index;
        }
        return 0;
    }
}

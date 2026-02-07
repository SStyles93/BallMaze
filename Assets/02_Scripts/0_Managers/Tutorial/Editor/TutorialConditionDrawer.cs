using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(ITutorialCondition), true)]
public class TutorialConditionDrawer : PropertyDrawer
{
    private static List<Type> conditionTypes;
    private static string[] displayNames;

    static TutorialConditionDrawer()
    {
        conditionTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                typeof(ITutorialCondition).IsAssignableFrom(t) &&
                !t.IsInterface &&
                !t.IsAbstract)
            .ToList();

        displayNames = new string[conditionTypes.Count + 1];
        displayNames[0] = "None";

        for (int i = 0; i < conditionTypes.Count; i++)
            displayNames[i + 1] = ObjectNames.NicifyVariableName(conditionTypes[i].Name);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.managedReferenceValue == null)
            return EditorGUIUtility.singleLineHeight;

        float height = EditorGUIUtility.singleLineHeight; // for dropdown
        SerializedProperty iterator = property.Copy();
        SerializedProperty end = iterator.GetEndProperty();

        iterator.NextVisible(true); // skip the managedReference field itself

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

        // Draw dropdown
        Rect dropdownRect = new Rect(
            position.x,
            position.y,
            position.width,
            EditorGUIUtility.singleLineHeight
        );

        int currentIndex = 0;
        if (property.managedReferenceValue != null)
        {
            Type currentType = property.managedReferenceValue.GetType();
            currentIndex = conditionTypes.IndexOf(currentType) + 1;
        }

        int newIndex = EditorGUI.Popup(dropdownRect, label.text, currentIndex, displayNames);

        if (newIndex != currentIndex)
        {
            if (newIndex == 0)
            {
                property.managedReferenceValue = null;
            }
            else
            {
                Type selectedType = conditionTypes[newIndex - 1];
                property.managedReferenceValue = Activator.CreateInstance(selectedType);
            }
        }

        // Draw child fields
        if (property.managedReferenceValue != null)
        {
            Rect contentRect = new Rect(
                position.x,
                position.y + EditorGUIUtility.singleLineHeight + 2,
                position.width,
                position.height - EditorGUIUtility.singleLineHeight - 2
            );

            SerializedProperty iterator = property.Copy();
            SerializedProperty end = iterator.GetEndProperty();

            iterator.NextVisible(true); // move to first child

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
}

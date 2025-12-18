using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomizationData", menuName = "Customization/CustomizationData")]
public class CustomizationData_SO : ScriptableObject
{
    public MaterialOption[] materials;
    public ColorOption[] colors;
}

public class CustomizationOption
{
    public bool isLocked;
    public CoinAmountPair price;
}

[System.Serializable]
public class MaterialOption : CustomizationOption
{
    public Material material;
    public Sprite sprite;
}

[System.Serializable]
public class ColorOption : CustomizationOption
{
    public string name;
    public Color color;
}

#if UNITY_EDITOR
[CustomEditor(typeof(CustomizationData_SO))]
public class CustomizationData_SO_CustomInspectior : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CustomizationData_SO _target = (CustomizationData_SO)target;

        if (GUILayout.Button("Reset Data"))
        {
            for (int i = 1; i < _target.materials.Length; i++)
            {
                _target.materials[i].isLocked = true;
            }
            for (int i = 1; i < _target.colors.Length; i++)
            {
                _target.colors[i].isLocked = true;
            }

            EditorUtility.SetDirty(_target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Unlock All"))
        {
            for (int i = 0; i < _target.materials.Length; i++)
            {
                _target.materials[i].isLocked = false;
            }
            for (int i = 0; i < _target.colors.Length; i++)
            {
                _target.colors[i].isLocked = false;
            }

            EditorUtility.SetDirty(_target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif
using MyBox;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomizationData", menuName = "Customization/CustomizationData")]
public class CustomizationData_SO : ScriptableObject
{
    public SkinOption[] skins;
    public ColorOption[] colors;
}

public class CustomizationOption
{
    public bool isColorable;
    public bool isLocked;
    public int levelToUnlock;
    public CoinQuantityPair price;
    public string name;
}

[System.Serializable]
public class SkinOption : CustomizationOption
{
    public GameObject skin;
    public Sprite sprite;
    [ConditionalField("isColorable", true)] public Color color = Color.white;
}

[System.Serializable]
public class ColorOption : CustomizationOption
{
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
            for (int i = 1; i < _target.skins.Length; i++)
            {
                _target.skins[i].isLocked = true;
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
            for (int i = 0; i < _target.skins.Length; i++)
            {
                _target.skins[i].isLocked = false;
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
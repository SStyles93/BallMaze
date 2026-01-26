using MyBox;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomizationData", menuName = "Customization/CustomizationData")]
public class CustomizationData_SO : ScriptableObject
{
    public SkinOption[] skins;
    public ColorOption[] colors;
}

[System.Serializable]
public class CustomizationOption
{
    [SerializeField] private string id;
    public bool isLocked;
    public int levelToUnlock;
    public CoinQuantityPair price;

    public string Id => id;
    public void SetID(string id) { this.id = id; }
}

[System.Serializable]
public class SkinOption : CustomizationOption
{
    public GameObject skin;
    public Sprite sprite;
    public bool isPremium = false;
    [ConditionalField("isPremium")] public Color color = Color.white;
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

        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;


        GUI.color = Color.green;
        if (GUILayout.Button("Reset Data", style, GUILayout.ExpandWidth(true)))
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

        GUI.color = Color.yellow;
        if (GUILayout.Button("Unlock All", style, GUILayout.ExpandWidth(true)))
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

        GUILayout.Space(20);

        GUI.color = Color.red;
        if (GUILayout.Button("Set All Skins ID", style, GUILayout.ExpandWidth(true)))
        {
            for (int i = 0; i < _target.skins.Length; i++)
            {
                _target.skins[i].SetID(_target.skins[i].skin.name.ToLower());
            }

            EditorUtility.SetDirty(_target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        GUI.color = Color.white;
    }
}
#endif
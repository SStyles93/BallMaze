using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSkin", menuName = "Customization/PlayerSkin")]
public class PlayerSkinData_SO : ScriptableObject
{
    [SerializeField] public SkinOption skinOption;
    [SerializeField] public int playerSkinIndex;
    [SerializeField] public ColorOption colorOption;
    [SerializeField] public int playerColorIndex;

    [SerializeField] private GameObject firstSkin;


#if UNITY_EDITOR
    [CustomEditor(typeof(PlayerSkinData_SO))]
    public class PlayerSkinData_SO_CustomInspectior : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            PlayerSkinData_SO _target = (PlayerSkinData_SO)target;

            if (GUILayout.Button("Reset Data"))
            {
                _target.skinOption.skin = _target.firstSkin;
                _target.playerSkinIndex = 0;

                _target.colorOption.color = Color.white;
                _target.playerColorIndex = 0;

                EditorUtility.SetDirty(_target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
#endif
}

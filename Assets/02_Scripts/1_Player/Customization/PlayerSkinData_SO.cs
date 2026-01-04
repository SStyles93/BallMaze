using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSkin", menuName = "Customization/PlayerSkin")]
public class PlayerSkinData_SO : ScriptableObject
{
    [SerializeField] public GameObject playerSkin;
    [SerializeField] public Color playerColor;
    [SerializeField] public int playerSkinIndex;
    [SerializeField] public int playerColorIndex;

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
                _target.playerSkin = null;
                _target.playerColor = Color.white;
                _target.playerSkinIndex = 0;
                _target.playerColorIndex = 0;

                EditorUtility.SetDirty(_target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
#endif
}

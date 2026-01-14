using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentColors", menuName = "Procedural Generation/Colors")]
public class EnvironmentColors_SO : ScriptableObject
{
    public ColorPreset[] Presets;
    public Material materialReference;
    public Material emissiveMaterialReference;

    public int targetPresetIndex;

    private int lastPresetIndex;

#if UNITY_EDITOR
    [CustomEditor(typeof(EnvironmentColors_SO))]
    public class EnvironmentColors_SO_CustomInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EnvironmentColors_SO _target = (EnvironmentColors_SO)target;

            if (GUILayout.Button("Add Material Colours as new Preset"))
            {
                ColorPreset[] currentPresets = _target.Presets;
                _target.Presets = new ColorPreset[currentPresets.Length+1];
                for (int i = 0; i < currentPresets.Length; i++)
                {
                    _target.Presets[i] = currentPresets[i];
                }
                _target.Presets[_target.Presets.Length - 1] =
                    new ColorPreset(
                        _target.materialReference.GetColor("_TopColor"),
                        _target.materialReference.GetColor("_RightColor"),
                        _target.materialReference.GetColor("_LeftColor"),
                        _target.materialReference.GetColor("_FrontColor"),
                        _target.emissiveMaterialReference.GetColor("_EmissionColor"));

                EditorUtility.SetDirty(_target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if(GUILayout.Button("Set Material to Preset"))
            {
                if (_target.targetPresetIndex > _target.Presets.Length-1 || _target.targetPresetIndex < 0) return;
                _target.materialReference.SetColor("_TopColor", _target.Presets[_target.targetPresetIndex].Top);
                _target.materialReference.SetColor("_RightColor", _target.Presets[_target.targetPresetIndex].Right);
                _target.materialReference.SetColor("_LeftColor", _target.Presets[_target.targetPresetIndex].Left);
                _target.materialReference.SetColor("_FrontColor", _target.Presets[_target.targetPresetIndex].Front);
                _target.emissiveMaterialReference.SetColor("_EmissionColor", _target.Presets[_target.targetPresetIndex].Emissive);
            }
        }
    }
#endif
}

[System.Serializable]
public class ColorPreset
{
    public Color Top = Color.white;
    public Color Right = Color.white;
    public Color Left = Color.white;
    public Color Front = Color.white;
    [ColorUsage(true,true)]
    public Color Emissive = Color.white;

    /// <summary>
    /// Consturctor for presets
    /// </summary>
    /// <param name="top"></param>
    /// <param name="right"></param>
    /// <param name="left"></param>
    /// <param name="front"></param>
    public ColorPreset(Color top, Color right, Color left, Color front, Color emissive)
    {
        Top = top;
        Right = right;
        Left = left;
        Front = front;
        Emissive = 
        Emissive = emissive;
    }
}

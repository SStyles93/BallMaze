using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentColors", menuName = "Procedural Generation/Colors")]
public class EnvironmentColors_SO : ScriptableObject
{
    public ColorPreset[] Presets;
}

[System.Serializable]
public class ColorPreset
{
    public Color Top = Color.white;
    public Color Right = Color.white;
    public Color Left = Color.white;
    public Color Front = Color.white;
}

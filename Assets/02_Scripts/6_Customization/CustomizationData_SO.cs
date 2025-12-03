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
    public int price;
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
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSkin", menuName = "Customization/PlayerSkin")]
public class PlayerSkinData_SO : ScriptableObject
{
    public Material playerMaterial;
    public Color playerColor;
    public int playerMaterialIndex;
    public int playerColorIndex;
}

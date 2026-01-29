using UnityEngine;

[CreateAssetMenu(menuName = "Procedural Generation/Tile Definition")]
public class TileDefinition_SO : ScriptableObject
{
    [Header("Identity")]
    public GroundType groundType;
    public OverlayType overlayType;

    [Header("Prefab")]
    public GameObject prefab;
    public Color editorColor = Color.pink;

    [Header("Generation")]
    [Range(0f, 1f)]
    public float ratio;
    public float maxRatio = 1f;
    public bool hasRatio = true;

    [Header("Rules")]
    public bool requiresThreeTiles;   // moving platforms
    public bool isWalkable = true;
    public bool allowsOverlay = true;

}

public enum GroundType
{
    Floor,
    Ice,
    MovingPlatformH, // horizontal
    MovingPlatformV, // vertical
    PlatformSide,
    Piques,
    Empty
    // **************************
    // ADD ANY GROUND TYPE HER
    // **************************
}

public enum OverlayType
{
    NONE,
    Start,
    End,
    Star
}


[System.Serializable]
public struct CellData
{
    public bool isEmpty;
    public GroundType ground;
    public OverlayType overlay;
}


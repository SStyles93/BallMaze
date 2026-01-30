using UnityEngine;

[CreateAssetMenu(fileName = "Tile Definition", menuName = "Procedural Generation/Tile Definition/Floor")]
public class TileDefinition_SO : ScriptableObject
{
    [Header("Tile Parameters")]
    [SerializeField] public GroundType groundType;
    [SerializeField] public GameObject tilePrefab;
    [SerializeField] public Color tileEditorColor;

    [Header("Rules")]
    [SerializeField] public bool isWalkable;
    [SerializeField] public bool awllowsOverlay;
}

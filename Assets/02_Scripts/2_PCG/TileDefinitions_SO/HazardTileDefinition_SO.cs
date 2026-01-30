using UnityEngine;

[CreateAssetMenu(fileName = "Hazard Tile Definition", menuName = "Procedural Generation/Tile Definition/Hazard")]
public class HazardTileDefinition_SO : TileDefinition_SO
{
    [Header("Ratio")]
    [SerializeField] public float ratio;
    [SerializeField] public float maxRatio;

    [Header("Special Rules")]
    [SerializeField] public bool requireTwoSolidNeighbours;
    [SerializeField] public bool isThreeLine;
}
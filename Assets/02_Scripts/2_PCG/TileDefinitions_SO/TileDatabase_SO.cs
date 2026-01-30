using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tile Database", menuName = "Procedural Generation/Tile Database")]
public class TileDatabase_SO : ScriptableObject
{
    [SerializeField] private TileDefinition_SO floorTile;

    [SerializeField] private List<HazardTileDefinition_SO> hazardTiles;

    [SerializeField] private List<OverlayDefinition_SO> overlays;

    Dictionary<GroundType, Color> groundColors;
    Dictionary<OverlayType, Color> overlayColors;

    public TileDefinition_SO FloorTile => floorTile;
    public List<HazardTileDefinition_SO> HazardTiles => hazardTiles;
    public List<OverlayDefinition_SO> Overlays { get => overlays; set => overlays = value; }

    public IEnumerable<TileDefinition_SO> GetTilesByGroudType(GroundType type)
     => new[] { floorTile }.Union(hazardTiles).Where(t => t.groundType == type);

    public IEnumerable<OverlayDefinition_SO> GetOverlaysByType(OverlayType type)
     => overlays.Where(t => t.overlayType == type);

    public IEnumerable<HazardTileDefinition_SO> GetTilesWithNonZeroRatio()
     => hazardTiles.Where(t => t.ratio > 0);

    void OnEnable()
    {
        BuildCaches();
    }

    void OnValidate()
    {
        BuildCaches();
    }

    void BuildCaches()
    {
        groundColors = new Dictionary<GroundType, Color>();
        overlayColors = new Dictionary<OverlayType, Color>();

        if (floorTile != null)
            groundColors[floorTile.groundType] = floorTile.tileEditorColor;

        foreach (var h in hazardTiles)
            groundColors[h.groundType] = h.tileEditorColor;

        foreach (var o in overlays)
            overlayColors[o.overlayType] = o.overlayEditorColor;
    }

    public Color GetGroundColor(GroundType type)
        => groundColors != null && groundColors.TryGetValue(type, out var c)
            ? c
            : Color.magenta;

    public Color GetOverlayColor(OverlayType type)
        => overlayColors != null && overlayColors.TryGetValue(type, out var c)
            ? c
            : Color.white;
}
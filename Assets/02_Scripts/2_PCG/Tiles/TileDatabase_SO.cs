using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Procedural Generation/Tile Database")]
public class TileDatabase_SO : ScriptableObject
{
    [SerializeField]
    private List<TileDefinition_SO> tiles = new();

    public List<TileDefinition_SO> Tiles => tiles;

    // -------------------------
    // DIRECT LOOKUPS
    // -------------------------
    public TileDefinition_SO GetByGround(GroundType type)
        => tiles.Find(t => t.groundType == type);

    public TileDefinition_SO GetByOverlay(OverlayType type)
        => tiles.Find(t => t.overlayType == type);

    // -------------------------
    // GENERATION QUERIES
    // -------------------------

    // Tiles that can replace base ground (Floor, Ice, Piques, etc.)
    public IEnumerable<TileDefinition_SO> GetRatioTiles()
        => tiles.Where(t => t.ratio > 0);

    public IEnumerable<TileDefinition_SO> GetBaseTiles()
        => tiles.Where(t => !t.requiresThreeTiles);

    // Tiles that require special placement logic (moving platforms)
    public IEnumerable<TileDefinition_SO> GetMultiTileTiles()
        => tiles.Where(t => t.requiresThreeTiles);

    // Tiles that can accept overlays (stars, start, end…)
    public IEnumerable<TileDefinition_SO> GetOverlayCompatibleTiles()
        => tiles.Where(t => t.allowsOverlay);

    // Walkable tiles only
    public IEnumerable<TileDefinition_SO> GetWalkableTiles()
        => tiles.Where(t => t.isWalkable);

    // -------------------------
    // VALIDATION
    // -------------------------

#if UNITY_EDITOR
    private void OnValidate()
    {
        float sum = 0f;

        foreach (var t in GetBaseTiles())
            sum += Mathf.Clamp01(t.ratio);

        if (sum > 1.01f)
            Debug.LogWarning(
                $"[TileDatabase] Base tile ratios exceed 1.0 ({sum})",
                this);
    }
#endif
}

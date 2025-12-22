using UnityEngine;

[System.Serializable]
public class LevelData_SO
{
    // Grid parameters
    public int gridWidth;
    public int gridHeight;

    // End position
    public bool randomEnd;
    public Vector2Int fixedEnd;
    public Vector2Int endMin;
    public Vector2Int endMax;

    // Seed
    public int inputSeed;

    // Path parameters
    public int pathThickness;
    public int curvePercent;

    // Stars
    public int minStarDistance;
    public bool starsConnectToEnd;

    // Grid and seed
    public TileType[,] grid;
    public int usedSeed;
}

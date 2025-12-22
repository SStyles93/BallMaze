using UnityEngine;

[System.Serializable]
public class LevelData_SO
{
    // Grid size
    public int width;
    public int height;


    // Generation parameters
    public int gridWidth;
    public int gridHeight;

    public bool randomEnd;
    public Vector2Int fixedEnd;
    public Vector2Int endMin;
    public Vector2Int endMax;

    public int inputSeed;
    public int pathThickness;
    public int curvePercent;

    public int starCount;
    public int minStarDistance;
    public bool starsConnectToEnd;

    // Flattened grid
    public TileType[] gridData;
    public int usedSeed;
}
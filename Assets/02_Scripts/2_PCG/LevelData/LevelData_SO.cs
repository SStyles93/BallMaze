using UnityEngine;

[System.Serializable]
public class LevelData_SO
{
    public int index;

    [Header("Grid Parameters")]
    // Generation parameters
    public int gridWidth;
    public int gridHeight;

    public bool randomEnd;
    public Vector2Int fixedEnd;
    public int endMinHeightPercent;
    public int inputSeed;

    [Header("Path Parameters")]
    public int pathThickness;
    public int curvePercent;

    [Header("Star & Currencies Parameters")]
    public int coinsToEarn;
    public int starCount;
    public int minStarDistance;
    public bool starsConnectToEnd;

    [Header("Grid Data")]
    // Flattened grid
    public TileType[] gridData;
    public int usedSeed;

    public TileType[,] ToGrid()
    {
        TileType[,] grid = new TileType[gridWidth, gridHeight];

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                grid[x, y] = gridData[y * gridWidth + x];
            }
        }

        return grid;
    }
}
using UnityEngine;

[System.Serializable]
public class LevelData_SO
{
    public int index;
    //public int currencyToEarn;

    // Generation parameters
    public int gridWidth;
    public int gridHeight;

    public bool randomEnd;
    public Vector2Int fixedEnd;
    public Vector2Int endMin = new Vector2Int(0,0);
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
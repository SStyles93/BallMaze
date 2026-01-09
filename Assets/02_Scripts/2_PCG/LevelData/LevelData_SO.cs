using UnityEngine;

[System.Serializable]
public class LevelData_SO
{
    public int index;

    [Header("Grid Parameters")]
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
    // Flattened Cell array
    public CellData[] gridData;
    public int usedSeed;

    /// <summary>
    /// Converts the flattened Cell array to a 2D grid
    /// </summary>
    public CellData[,] ToGrid()
    {
        CellData[,] grid = new CellData[gridWidth, gridHeight];

        if (gridData == null || gridData.Length != gridWidth * gridHeight)
        {
            Debug.LogWarning("LevelData_SO: gridData is null or has wrong length. Returning empty grid.");
            return grid;
        }

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                grid[x, y] = gridData[y * gridWidth + x];
            }
        }

        return grid;
    }

    /// <summary>
    /// Converts a 2D Cell grid into the flattened gridData array
    /// </summary>
    public void FromGrid(CellData[,] grid)
    {
        if (grid == null)
        {
            gridData = null;
            return;
        }

        gridWidth = grid.GetLength(0);
        gridHeight = grid.GetLength(1);

        gridData = new CellData[gridWidth * gridHeight];

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                gridData[y * gridWidth + x] = grid[x, y];
            }
        }
    }
}

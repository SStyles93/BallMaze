using System;
using UnityEngine;

[Serializable]
public struct TileRatioData
{
    public GroundType groundType;
    public float ratio;
}

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
    public float iceRatio;
    public float piqueRatio;
    public float doorDownRatio;
    public float doorUpRatio;
    public float movingPlatformRatio;
    public float emptyRatio;

    [Header("Star & Currencies Parameters")]
    public int coinsToEarn;
    public int starCount;
    public int minStarDistance;
    public bool starsConnectToEnd;

    [Header("Grid Data")]
    [Tooltip("Flattened grid: index = y * width + x")]
    public CellData[] gridData;

    public int usedSeed;

    // --------------------------------------------------
    // Grid conversion
    // --------------------------------------------------

    /// <summary>
    /// Converts the flattened Cell array into a Grid
    /// </summary>
    public Grid ToGrid()
    {
        if (gridWidth <= 0 || gridHeight <= 0)
        {
            Debug.LogWarning(
                $"LevelData_SO: Invalid grid size ({gridWidth}x{gridHeight}). Returning empty grid.");
            return new Grid(0, 0);
        }

        Grid grid = new Grid(gridWidth, gridHeight);

        if (gridData == null || gridData.Length != gridWidth * gridHeight)
        {
            Debug.LogWarning(
                $"LevelData_SO: gridData invalid (len = {gridData?.Length ?? 0}, expected {gridWidth * gridHeight}). Filling defaults.");
            return grid;
        }

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                ref var cell = ref grid.GetCellRef(x, y);
                cell = gridData[y * gridWidth + x];
            }
        }

        return grid;
    }

    /// <summary>
    /// Converts a Grid into the flattened gridData array
    /// </summary>
    public void FromGrid(Grid grid)
    {
        if (grid == null)
        {
            gridData = null;
            return;
        }

        gridWidth = grid.Width;
        gridHeight = grid.Height;

        gridData = new CellData[gridWidth * gridHeight];

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                gridData[y * gridWidth + x] = grid.GetCell(x, y);
            }
        }
    }
}

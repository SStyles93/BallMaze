using System.Collections.Generic;
using UnityEngine;

public enum GroundType
{
    Floor,
    Ice
}

public enum OverlayType
{
    None,
    Start,
    End,
    Star
}

[System.Serializable]
public struct CellData
{
    public bool isWall;
    public GroundType ground;
    public OverlayType overlay;
}

public static class Generator
{
    // =========================================================
    // PUBLIC ENTRY POINT
    // =========================================================

    /// <summary>
    /// Gets the parameters passed and creates a maze
    /// </summary>
    /// <param name="p">GeneratorParameters to pass</param>
    /// <param name="usedSeed">usedSeed</param>
    /// <returns>TIleType Array</returns>
    public static CellData[,] GenerateMaze(
    GeneratorParameters_SO p,
    out int usedSeed
)
    {
        usedSeed = p.inputSeed == -1 ? 
            Random.Range(int.MinValue, int.MaxValue) : p.inputSeed;

        System.Random rng = new System.Random(usedSeed);

        Vector2Int start = GetStartPosition(p);
        Vector2Int end = ResolveEndPosition(p, rng);

        CellData[,] grid = CreateWallGrid(p.gridWidth, p.gridHeight);

        List<Vector2Int> mainPath = GeneratePath(
            p.gridWidth,
            p.gridHeight,
            start,
            end,
            rng,
            p.curvePercent
        );

        if (mainPath == null || mainPath.Count == 0)
        {
            Debug.LogError("Failed to generate main path.");
            return grid;
        }

        CarvePath(grid, mainPath, p.pathThickness);
        MarkStartAndEnd(grid, start, end);

        PlaceStarsAndPaths(
            grid,
            p,
            mainPath,
            start,
            end,
            rng
        );

        return grid;
    }

    // =========================================================
    // START & END
    // =========================================================

    /// <summary>
    /// Start is ALWAYS bottom-center of the grid.
    /// </summary>
    private static Vector2Int GetStartPosition(GeneratorParameters_SO p)
    {
        return new Vector2Int(
            p.gridWidth / 2,
            p.gridHeight - 1
        );
    }

    private static Vector2Int ResolveEndPosition(
        GeneratorParameters_SO p,
        System.Random rng
    )
    {
        int width = p.gridWidth;
        int height = p.gridHeight;

        // Clamp percentage
        int percent = Mathf.Clamp(p.endMaxHeightPercent, 0, 100);

        // Compute max allowed Y based on percentage
        int maxAllowedY = Mathf.FloorToInt(height * (percent / 100f)) - 1;
        maxAllowedY = Mathf.Clamp(maxAllowedY, 0, height - 1);

        if (!p.randomEnd)
        {
            // Fixed end, but clamped to allowed range
            Vector2Int end = p.fixedEnd;

            end.x = Mathf.Clamp(end.x, 0, width - 1);
            end.y = Mathf.Clamp(end.y, 0, maxAllowedY);

            return end;
        }

        // Random end
        int x = rng.Next(0, width);
        int y = rng.Next(0, maxAllowedY + 1);

        return new Vector2Int(x, y);
    }

    // =========================================================
    // PATH GENERATION
    // =========================================================

    private static List<Vector2Int> GeneratePath(
        int width,
        int height,
        Vector2Int start,
        Vector2Int end,
        System.Random rng,
        int curvePercent
    )
    {
        start = ClampToGrid(start, width, height);
        end = ClampToGrid(end, width, height);

        List<Vector2Int> path = new();
        HashSet<Vector2Int> visited = new();

        path.Add(start);
        visited.Add(start);

        while (path[^1] != end)
        {
            Vector2Int current = path[^1];
            List<Vector2Int> neighbors =
                GetUnvisitedNeighbors(current, width, height, visited, rng);

            if (neighbors.Count == 0)
            {
                path.RemoveAt(path.Count - 1);
                if (path.Count == 0)
                    return null;
                continue;
            }

            Vector2Int next =
                rng.Next(0, 100) < curvePercent ? 
                neighbors[rng.Next(neighbors.Count)] : PickClosest(neighbors, end);

            path.Add(next);
            visited.Add(next);
        }

        return path;
    }

    private static List<Vector2Int> GetUnvisitedNeighbors(
        Vector2Int cell,
        int width,
        int height,
        HashSet<Vector2Int> visited,
        System.Random rng
    )
    {
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        List<Vector2Int> neighbors = new();

        foreach (var d in directions)
        {
            Vector2Int n = cell + d;
            if (IsInsideGrid(n, width, height) && !visited.Contains(n))
                neighbors.Add(n);
        }

        Shuffle(neighbors, rng);
        return neighbors;
    }

    private static Vector2Int PickClosest(
        List<Vector2Int> cells,
        Vector2Int target
    )
    {
        Vector2Int best = cells[0];
        float bestDist = Vector2Int.Distance(best, target);

        foreach (var c in cells)
        {
            float d = Vector2Int.Distance(c, target);
            if (d < bestDist)
            {
                best = c;
                bestDist = d;
            }
        }

        return best;
    }

    private static void Shuffle<T>(
        List<T> list,
        System.Random rng
    )
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = rng.Next(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // =========================================================
    // GRID CARVING
    // =========================================================

    private static CellData[,] CreateWallGrid(int width, int height)
    {
        CellData[,] grid = new CellData[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new CellData
                {
                    isWall = true,
                    ground = GroundType.Floor,
                    overlay = OverlayType.None
                };
            }
        }

        return grid;
    }

    private static void CarvePath(
    CellData[,] grid,List<Vector2Int> path,int thickness)
    {
        foreach (var cell in path)
            CarveCell(grid, cell, thickness);
    }

    private static void CarveCell(
    CellData[,] grid, Vector2Int center, int thickness)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        for (int dx = -thickness; dx <= thickness; dx++)
        {
            for (int dy = -thickness; dy <= thickness; dy++)
            {
                Vector2Int p = new(center.x + dx, center.y + dy);

                if (!IsInsideGrid(p, width, height))
                    continue;

                ref CellData cell = ref grid[p.x, p.y];

                // Never overwrite overlays
                if (cell.overlay != OverlayType.None)
                    continue;

                cell.isWall = false;
                cell.ground = GroundType.Floor;
            }
        }
    }


    private static void MarkStartAndEnd(CellData[,] grid, 
        Vector2Int start, Vector2Int end)
    {
        if (IsInsideGrid(start, grid))
        {
            grid[start.x, start.y].isWall = false;
            grid[start.x, start.y].ground = GroundType.Floor;
            grid[start.x, start.y].overlay = OverlayType.Start;
        }

        if (IsInsideGrid(end, grid))
        {
            grid[end.x, end.y].isWall = false;
            grid[end.x, end.y].ground = GroundType.Floor;
            grid[end.x, end.y].overlay = OverlayType.End;
        }
    }

    // =========================================================
    // STARS
    // =========================================================

    private static void PlaceStarsAndPaths(
    CellData[,] grid,
    GeneratorParameters_SO p,
    List<Vector2Int> mainPath,
    Vector2Int start,
    Vector2Int end,
    System.Random rng
)
    {
        List<Vector2Int> placedStars = new();
        List<Vector2Int> freeCells = new();

        for (int x = 0; x < p.gridWidth; x++)
        {
            for (int y = 0; y < p.gridHeight; y++)
            {
                Vector2Int pos = new(x, y);
                if (pos != start && pos != end)
                    freeCells.Add(pos);
            }
        }

        Shuffle(freeCells, rng);

        int starsPlaced = 0;

        foreach (var pos in freeCells)
        {
            bool tooClose = false;
            foreach (var s in placedStars)
            {
                if (Vector2Int.Distance(pos, s) < p.minStarDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose)
                continue;

            ref CellData cell = ref grid[pos.x, pos.y];

            if (cell.isWall)
                continue;

            placedStars.Add(pos);
            cell.overlay = OverlayType.Star;

            Vector2Int connection = mainPath[rng.Next(mainPath.Count)];
            List<Vector2Int> starPath = GeneratePath(
                p.gridWidth,
                p.gridHeight,
                connection,
                pos,
                rng,
                p.curvePercent
            );

            if (starPath != null)
                CarvePath(grid, starPath, p.pathThickness);

            if (p.starsConnectToEnd)
            {
                List<Vector2Int> endPath = GeneratePath(
                    p.gridWidth,
                    p.gridHeight,
                    pos,
                    end,
                    rng,
                    p.curvePercent
                );

                if (endPath != null)
                    CarvePath(grid, endPath, p.pathThickness);
            }

            starsPlaced++;
            if (starsPlaced >= p.starCount)
                break;
        }
    }

    // =========================================================
    // UTILITIES
    // =========================================================

    private static bool IsInsideGrid(
        Vector2Int p,
        int width,
        int height
    )
    {
        return p.x >= 0 && p.x < width &&
               p.y >= 0 && p.y < height;
    }

    private static bool IsInsideGrid(Vector2Int p, CellData[,] grid)
    {
        return IsInsideGrid(
            p,
            grid.GetLength(0),
            grid.GetLength(1)
        );
    }

    private static Vector2Int ClampToGrid(
        Vector2Int p,
        int width,
        int height
    )
    {
        return new Vector2Int(
            Mathf.Clamp(p.x, 0, width - 1),
            Mathf.Clamp(p.y, 0, height - 1)
        );
    }
}

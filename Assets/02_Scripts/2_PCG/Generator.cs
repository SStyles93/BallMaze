using System.Collections.Generic;
using UnityEngine;

public enum GroundType
{
    Floor,
    Ice,
    MovingPlatformH, // horizontal
    MovingPlatformV, // vertical
    PlatformSide
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
    GeneratorParameters_SO p, out int usedSeed)
    {
        usedSeed = p.inputSeed == -1 ?
            Random.Range(int.MinValue, int.MaxValue) : p.inputSeed;

        System.Random rng = new System.Random(usedSeed);

        Vector2Int start = GetStartPosition(p);
        Vector2Int end = ResolveEndPosition(p, rng);

        CellData[,] grid = CreateWallGrid(p.gridWidth, p.gridHeight);

        List<Vector2Int> mainPath = GeneratePath(
            p.gridWidth, p.gridHeight,
            start,end,
            rng, p
        );

        if (mainPath == null || mainPath.Count == 0)
        {
            Debug.LogError("Failed to generate main path.");
            return grid;
        }

        CarvePath(grid, mainPath, p);
        MarkStartAndEnd(grid,start, end);

        PlaceStarsAndPaths(grid, p, mainPath, 
            start, end, rng);

        ApplyEmptyTiles(grid, p, 
            start, end, rng);

        PlaceMovingPlatforms(grid, p, rng);

        return grid;
    }

    // === START & END ===

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
        GeneratorParameters_SO p,System.Random rng)
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

    // === PATH GENERATION ===

    private static List<Vector2Int> GeneratePath(
        int width, int height,
        Vector2Int start, Vector2Int end,
        System.Random rng, GeneratorParameters_SO p)
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
                rng.Next(0, 100) < p.curvePercent ?
                neighbors[rng.Next(neighbors.Count)] : PickClosest(neighbors, end);

            path.Add(next);
            visited.Add(next);
        }

        return path;
    }

    private static List<Vector2Int> GetUnvisitedNeighbors(
        Vector2Int cell, int width, int height,
        HashSet<Vector2Int> visited, System.Random rng)
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
        List<Vector2Int> cells,Vector2Int target)
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
        List<T> list, System.Random rng)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = rng.Next(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    
    // === GRID CARVING ===

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

    private static void CarvePath(CellData[,] grid, 
        List<Vector2Int> path, GeneratorParameters_SO p)
    {
        foreach (var cell in path)
            CarveCell(grid, cell, p);
    }

    private static void CarveCell(CellData[,] grid, 
        Vector2Int center, GeneratorParameters_SO p)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        int pathThickness = p.pathThickness;

        for (int dx = -pathThickness; dx <= pathThickness; dx++)
        {
            for (int dy = -pathThickness; dy <= pathThickness; dy++)
            {
                Vector2Int path = new(center.x + dx, center.y + dy);

                if (!IsInsideGrid(path, width, height))
                    continue;

                ref CellData cell = ref grid[path.x, path.y];

                // Never overwrite overlays
                if (cell.overlay != OverlayType.None)
                    continue;

                cell.isWall = false;
                cell.ground = (Random.value < p.iceRatio) ? GroundType.Ice : GroundType.Floor;
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

    private static void ApplyEmptyTiles(CellData[,] grid, 
        GeneratorParameters_SO p, Vector2Int start, Vector2Int end, 
        System.Random rng)
    {
        if (p.emptyRatio <= 0f)
            return;

        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        List<Vector2Int> candidates = new();

        // Collect ONLY walkable path tiles
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                ref CellData cell = ref grid[x, y];

                if (cell.isWall)
                    continue;

                if (cell.overlay != OverlayType.None)
                    continue;

                Vector2Int pos = new(x, y);

                // Never near Start or End
                if (IsNear(pos, start) || IsNear(pos, end))
                    continue;

                candidates.Add(pos);
            }
        }

        Shuffle(candidates, rng);

        int targetCount =
            Mathf.RoundToInt(candidates.Count * p.emptyRatio);

        int placed = 0;

        foreach (var pos in candidates)
        {
            if (placed >= targetCount)
                break;

            if (HasAdjacentEmpty(grid, pos))
                continue;

            ref CellData cell = ref grid[pos.x, pos.y];

            cell.isWall = true;
            cell.overlay = OverlayType.None;

            placed++;
        }
    }

    static void PlaceMovingPlatforms(CellData[,] grid,
    GeneratorParameters_SO p, System.Random rng)
    {
        if (p.movingPlatformRatio <= 0f)
            return;

        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        // Count total walkable tiles (non-wall) excluding start/end overlays
        int totalWalkableTiles = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var cell = grid[x, y];
                if (!cell.isWall && cell.overlay != OverlayType.Start && cell.overlay != OverlayType.End)
                    totalWalkableTiles++;
            }
        }

        // Estimate max platforms (each takes 3 tiles)
        int maxPlatforms = Mathf.Max(1, Mathf.RoundToInt(totalWalkableTiles * p.movingPlatformRatio / 3f));

        List<(Vector2Int center, bool horizontal)> candidates = new();

        // Find all valid centers for platforms
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                var cell = grid[x, y];

                // Skip walls, start or end positions
                if (cell.isWall || cell.overlay == OverlayType.Start || cell.overlay == OverlayType.End)
                    continue;

                Vector2Int center = new(x, y);

                // Horizontal platform check: 3 consecutive walkable tiles (center + sides) excluding start/end
                if (!grid[x - 1, y].isWall && !grid[x + 1, y].isWall &&
                    grid[x - 1, y].overlay != OverlayType.Start && grid[x - 1, y].overlay != OverlayType.End &&
                    grid[x + 1, y].overlay != OverlayType.Start && grid[x + 1, y].overlay != OverlayType.End)
                {
                    candidates.Add((center, true));
                }

                // Vertical platform check: 3 consecutive walkable tiles (center + sides) excluding start/end
                if (!grid[x, y - 1].isWall && !grid[x, y + 1].isWall &&
                    grid[x, y - 1].overlay != OverlayType.Start && grid[x, y - 1].overlay != OverlayType.End &&
                    grid[x, y + 1].overlay != OverlayType.Start && grid[x, y + 1].overlay != OverlayType.End)
                {
                    candidates.Add((center, false));
                }
            }
        }

        Shuffle(candidates, rng);

        // Keep track of tiles already reserved for platforms
        bool[,] reserved = new bool[width, height];

        int placed = 0;

        foreach (var c in candidates)
        {
            if (placed >= maxPlatforms)
                break;

            Vector2Int center = c.center;

            // Skip if any of the 3 tiles are already reserved
            bool overlap = false;
            if (c.horizontal)
            {
                overlap = reserved[center.x - 1, center.y] ||
                          reserved[center.x, center.y] ||
                          reserved[center.x + 1, center.y];
            }
            else
            {
                overlap = reserved[center.x, center.y - 1] ||
                          reserved[center.x, center.y] ||
                          reserved[center.x, center.y + 1];
            }

            if (overlap)
                continue;

            ref CellData centerCell = ref grid[center.x, center.y];

            // Skip if center is start or end (safety check)
            if (centerCell.overlay == OverlayType.Start || centerCell.overlay == OverlayType.End)
                continue;

            // Center becomes the moving platform
            centerCell.isWall = false;
            centerCell.ground = c.horizontal ? GroundType.MovingPlatformH : GroundType.MovingPlatformV;

            // Side tiles become PlatformSide (preserve any stars)
            if (c.horizontal)
            {
                SetPlatformSide(grid, center.x - 1, center.y);
                SetPlatformSide(grid, center.x + 1, center.y);

                // Reserve the 3 tiles
                ReserveTile(reserved, center.x - 1, center.y);
                ReserveTile(reserved, center.x, center.y);
                ReserveTile(reserved, center.x + 1, center.y);
            }
            else
            {
                SetPlatformSide(grid, center.x, center.y - 1);
                SetPlatformSide(grid, center.x, center.y + 1);

                // Reserve the 3 tiles
                ReserveTile(reserved, center.x, center.y - 1);
                ReserveTile(reserved, center.x, center.y);
                ReserveTile(reserved, center.x, center.y + 1);
            }

            placed++;
        }
    }

    // Marks a tile as PlatformSide without removing stars
    private static void SetPlatformSide(CellData[,] grid, int x, int y)
    {
        ref CellData cell = ref grid[x, y];
        if (cell.ground != GroundType.MovingPlatformH && cell.ground != GroundType.MovingPlatformV)
        {
            cell.isWall = false;        // optional: ensures it's walkable
            cell.ground = GroundType.PlatformSide;
        }
    }

    // Marks a tile as reserved for placement
    private static void ReserveTile(bool[,] reserved, int x, int y)
    {
        reserved[x, y] = true;
    }



    // === STARS ===

    private static void PlaceStarsAndPaths(
    CellData[,] grid, GeneratorParameters_SO p, List<Vector2Int> mainPath,
    Vector2Int start, Vector2Int end, 
    System.Random rng)
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
        var freeCellsSnapshot = new List<Vector2Int>(freeCells);

        foreach (var pos in freeCellsSnapshot)
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
            var starPath = GeneratePath(
                p.gridWidth,
                p.gridHeight,
                connection,
                pos,
                rng,
                p
            );

            if (starPath != null)
                CarvePath(grid, starPath, p);

            if (p.starsConnectToEnd)
            {
                var endPath = GeneratePath(
                    p.gridWidth,
                    p.gridHeight,
                    pos,
                    end,
                    rng,
                    p
                );

                if (endPath != null)
                    CarvePath(grid, endPath, p);
            }

            starsPlaced++;
            if (starsPlaced >= p.starCount)
                break;
        }

        // Fallback
        if (starsPlaced < p.starCount)
        {
            ForcePlaceMissingStars(
                grid,
                freeCells,
                placedStars,
                p.starCount - starsPlaced,
                rng
            );
        }
    }

    // Distance relaxation 
    private static void ForcePlaceMissingStars(
    CellData[,] grid, 
    List<Vector2Int> candidates, List<Vector2Int> placedStars,
    int missingCount, System.Random rng)
    {
        // Start by relaxing distance gradually
        int relaxedDistance = Mathf.Max(1, placedStars.Count > 0 ? 2 : 1);

        Shuffle(candidates, rng);

        foreach (var pos in candidates)
        {
            if (missingCount <= 0)
                return;

            ref CellData cell = ref grid[pos.x, pos.y];

            if (cell.isWall || cell.overlay != OverlayType.None)
                continue;

            bool tooClose = false;
            foreach (var s in placedStars)
            {
                if (Vector2Int.Distance(pos, s) < relaxedDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose)
                continue;

            // Place forced star
            cell.overlay = OverlayType.Star;
            placedStars.Add(pos);
            missingCount--;
        }
    }

    // === UTILITIES ===

    private static bool IsInsideGrid(Vector2Int p, int width, int height)
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

    private static bool IsNear(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) <= 1 &&
               Mathf.Abs(a.y - b.y) <= 1;
    }

    private static Vector2Int ClampToGrid(Vector2Int p, int width, int height)
    {
        return new Vector2Int(
            Mathf.Clamp(p.x, 0, width - 1),
            Mathf.Clamp(p.y, 0, height - 1)
        );
    }

    private static bool HasAdjacentEmpty(CellData[,] grid, Vector2Int pos)
    {
        Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var d in dirs)
        {
            Vector2Int n = pos + d;

            if (!IsInsideGrid(n, grid))
                continue;

            if (grid[n.x, n.y].isWall)
                return true;
        }

        return false;
    }
}

using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathGenerator
{
    // --- Member Variables ---
    private GenerationParams p;
    private int step;
    private Vector2Int gridSize;
    private Vector2Int startPos;
    private Vector2Int endPos;
    private CellType[,] grid;
    private System.Random random;

    // --- Main Public Method ---
    public CellType[,] Generate(GenerationParams parameters)
    {
        // 1. Initialization
        this.p = parameters;
        // NEW: Ensure PathWidth is at least 1
        if (this.p.PathWidth < 1) this.p.PathWidth = 1;
        this.step = p.Spacing + 1;
        if (p.Seed != -1) random = new System.Random(p.Seed);
        else random = new System.Random((int)System.DateTime.Now.Ticks);

        CalculateGridSize();
        this.startPos = SanitizeCoord(p.StartPos);
        if (p.PlaceEndManually)
        {
            this.endPos = SanitizeCoord(p.EndPos);
        }

        grid = new CellType[gridSize.x, gridSize.y];
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                grid[x, y] = CellType.Wall;
            }
        }

        // --- Path Generation ---
        GenerateRandomizedSolutionPath();
        GenerateFillerPaths();
        if (p.AllowBranching)
        {
            AddDeadEndBranches();
        }

        // --- End Point Placement ---
        if (!p.PlaceEndManually)
        {
            PlaceEndAtFurthestPoint();
        }
        else
        {
            ConnectToManualEndPoint();
        }

        // --- Finalization ---
        // MODIFIED: Use CarveThickPath for start and end points to ensure they are also widened.
        CarveThickPath(startPos, CellType.Start);
        if (IsWithinBounds(endPos))
        {
            CarveThickPath(endPos, CellType.End);
        }

        // Ensure the exact start/end points are marked correctly over any widened path
        grid[startPos.x, startPos.y] = CellType.Start;
        if (IsWithinBounds(endPos)) grid[endPos.x, endPos.y] = CellType.End;


        return grid;
    }

    // --- Core Path Generation & Carving Methods (Unchanged) ---
    // The logic of *finding* the path is the same. Only the carving methods below are changed.
    private void GenerateRandomizedSolutionPath()
    {
        var current = startPos;
        var path = new List<Vector2Int>();
        var pathSet = new HashSet<Vector2Int>();
        Vector2Int lastMoveDirection = Vector2Int.zero;
        int iterations = (gridSize.x * gridSize.y) / (step + 1);

        for (int i = 0; i < iterations; i++)
        {
            path.Add(current);
            pathSet.Add(current);
            CarvePath(current);
            var possibleMoves = GetValidNeighbors(current, false, pathSet);
            if (possibleMoves.Count == 0)
            {
                if (path.Count > 1) { current = path[path.Count - 2]; path.RemoveAt(path.Count - 1); continue; } else break;
            }
            var moveScores = new Dictionary<Vector2Int, float>();
            foreach (var move in possibleMoves)
            {
                float score = 0;
                Vector2Int moveDirection = (move - current) / step;
                if (moveDirection == lastMoveDirection) score += 100 - p.PathTwistiness;
                else if (moveDirection == -lastMoveDirection) score -= 50;
                else score += p.PathTwistiness;
                score += random.Next(0, 20);
                moveScores[move] = score;
            }
            Vector2Int nextMove = moveScores.OrderByDescending(kvp => kvp.Value).First().Key;
            lastMoveDirection = (nextMove - current) / step;
            CarvePathBetween(current, nextMove);
            current = nextMove;
        }
    }

    private void GenerateFillerPaths()
    {
        var allCarvedCells = GetAllCellsOfType(CellType.Path, CellType.Start);
        if (allCarvedCells.Count == 0) return;
        int maxCells = (gridSize.x / step) * (gridSize.y / step);
        int targetCellCount = Mathf.RoundToInt(maxCells * (p.PathDensity / 100.0f));
        int cellsToCarve = targetCellCount - allCarvedCells.Count;
        Shuffle(allCarvedCells);
        var frontier = new List<Vector2Int>(allCarvedCells);
        while (cellsToCarve > 0 && frontier.Count > 0)
        {
            var current = frontier.Last();
            var neighbors = GetValidNeighbors(current, allowOccupied: false);
            if (neighbors.Count > 0)
            {
                var next = neighbors[random.Next(neighbors.Count)];
                CarvePathBetween(current, next);
                frontier.Add(next);
                cellsToCarve -= (step > 0 ? step : 1);
            }
            else { frontier.RemoveAt(frontier.Count - 1); }
        }
    }

    private void AddDeadEndBranches()
    {
        var pathCells = GetAllCellsOfType(CellType.Path, CellType.Start);
        if (pathCells.Count == 0) return;
        int branchesToAttempt = Mathf.FloorToInt(pathCells.Count * 0.25f);
        Shuffle(pathCells);
        for (int i = 0; i < branchesToAttempt; i++)
        {
            var cell = pathCells[i];
            var neighbors = GetValidNeighbors(cell, allowOccupied: false);
            if (neighbors.Count > 0)
            {
                var branchTo = neighbors[random.Next(neighbors.Count)];
                int branchLength = random.Next(1, 4) * step;
                var current = cell;
                for (int j = 0; j < branchLength && IsWithinBounds(branchTo); j += step)
                {
                    if (grid[branchTo.x, branchTo.y] != CellType.Wall) break;
                    CarvePathBetween(current, branchTo);
                    current = branchTo;
                    var nextNeighbors = GetValidNeighbors(current, false);
                    if (nextNeighbors.Count == 0) break;
                    branchTo = nextNeighbors[random.Next(nextNeighbors.Count)];
                }
            }
        }
    }

    // --- End Point Placement & Connection Methods (Unchanged) ---
    private void PlaceEndAtFurthestPoint()
    {
        var allPathCells = GetAllCellsOfType(CellType.Path);
        if (allPathCells.Count == 0) { endPos = startPos; return; }
        var terminalNodes = new List<Vector2Int>();
        var directions = new[] { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0) };
        foreach (var cell in allPathCells)
        {
            int carvedNeighborCount = 0;
            foreach (var dir in directions)
            {
                var neighbor = cell + dir;
                if (IsWithinBounds(neighbor) && grid[neighbor.x, neighbor.y] != CellType.Wall) carvedNeighborCount++;
            }
            if (carvedNeighborCount == 1) terminalNodes.Add(cell);
        }
        var eligibleTerminalNodes = terminalNodes.Where(cell => Vector2.Distance(startPos, cell) >= p.MinEndDistance).ToList();
        if (eligibleTerminalNodes.Count == 1) { this.endPos = eligibleTerminalNodes[0]; return; }
        else if (eligibleTerminalNodes.Count > 1) { this.endPos = eligibleTerminalNodes.OrderByDescending(cell => Vector2.Distance(startPos, cell)).First(); return; }
        else
        {
            var eligiblePathCells = allPathCells.Where(cell => Vector2.Distance(startPos, cell) >= p.MinEndDistance).ToList();
            var searchList = eligiblePathCells.Count > 0 ? eligiblePathCells : allPathCells;
            this.endPos = searchList.OrderByDescending(cell => Vector2.Distance(startPos, cell)).FirstOrDefault();
        }
    }

    private void ConnectToManualEndPoint()
    {
        if (!IsWithinBounds(endPos) || grid[endPos.x, endPos.y] != CellType.Wall) return;
        var allPathCells = GetAllCellsOfType(CellType.Path, CellType.Start);
        if (allPathCells.Count == 0) return;
        Vector2Int closestCell = allPathCells.OrderBy(cell => Vector2.Distance(cell, endPos)).First();
        var current = closestCell;
        while (current != endPos)
        {
            var next = current;
            if (Mathf.Abs(endPos.x - current.x) > Mathf.Abs(endPos.y - current.y)) next.x += (endPos.x > current.x ? step : -step);
            else next.y += (endPos.y > current.y ? step : -step);
            CarvePathBetween(current, next);
            current = next;
        }
    }

    // --- Helper & Utility Methods ---
    private List<Vector2Int> GetValidNeighbors(Vector2Int pos, bool allowOccupied, HashSet<Vector2Int> exclude = null)
    {
        var neighbors = new List<Vector2Int>();
        var directions = new[] { new Vector2Int(0, step), new Vector2Int(0, -step), new Vector2Int(step, 0), new Vector2Int(-step, 0) };
        foreach (var dir in directions)
        {
            var neighbor = pos + dir;
            if (IsWithinBounds(neighbor) && (exclude == null || !exclude.Contains(neighbor)))
            {
                if (allowOccupied || grid[neighbor.x, neighbor.y] == CellType.Wall) neighbors.Add(neighbor);
            }
        }
        return neighbors;
    }

    private List<Vector2Int> GetAllCellsOfType(params CellType[] types)
    {
        var cells = new List<Vector2Int>();
        var typeSet = new HashSet<CellType>(types);
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                if (typeSet.Contains(grid[x, y])) cells.Add(new Vector2Int(x, y));
            }
        }
        return cells;
    }

    private void CalculateGridSize() { gridSize = new Vector2Int(p.MaxX, p.MaxZ); }

    private Vector2Int SanitizeCoord(Vector2Int pos)
    {
        int validX = Mathf.RoundToInt((float)pos.x / step) * step;
        int validY = Mathf.RoundToInt((float)pos.y / step) * step;
        validX = Mathf.Clamp(validX, 0, gridSize.x - 1);
        validY = Mathf.Clamp(validY, 0, gridSize.y - 1);
        return new Vector2Int(validX, validY);
    }

    private bool IsWithinBounds(Vector2Int pos) => pos.x >= 0 && pos.x < gridSize.x && pos.y >= 0 && pos.y < gridSize.y;

    // --- MODIFIED & NEW Carving Methods ---

    // MODIFIED: This is now a simple wrapper around the new CarveThickPath method.
    private void CarvePath(Vector2Int pos)
    {
        CarveThickPath(pos, CellType.Path);
    }

    // MODIFIED: This now calls CarveThickPath for every point along the line.
    private void CarvePathBetween(Vector2Int from, Vector2Int to)
    {
        Vector2Int diff = to - from;
        Vector2Int dir = new Vector2Int(Mathf.Clamp(diff.x, -1, 1), Mathf.Clamp(diff.y, -1, 1));
        int distance = (int)Mathf.Max(Mathf.Abs(diff.x), Mathf.Abs(diff.y));
        for (int i = 0; i <= distance; i++)
        {
            CarveThickPath(from + dir * i, CellType.Path);
        }
    }

    // NEW: This is the core method for creating wide paths.
    // It carves a square of cells around a central point.
    private void CarveThickPath(Vector2Int center, CellType type)
    {
        // PathWidth=1 -> radius=0 (1x1 square)
        // PathWidth=2 -> radius=1 (3x3 square)
        // PathWidth=3 -> radius=2 (5x5 square)
        int radius = p.PathWidth - 1;

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector2Int pointToCarve = center + new Vector2Int(x, y);
                if (IsWithinBounds(pointToCarve))
                {
                    // Only carve if it's a wall. Don't overwrite existing Start/End points.
                    if (grid[pointToCarve.x, pointToCarve.y] == CellType.Wall)
                    {
                        grid[pointToCarve.x, pointToCarve.y] = type;
                    }
                }
            }
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int k = random.Next(i + 1);
            (list[k], list[i]) = (list[i], list[k]);
        }
    }
}

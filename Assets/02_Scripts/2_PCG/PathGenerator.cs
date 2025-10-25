using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathGenerator
{
    // --- Member Variables ---
    private GenerationParamameters_SO p;
    private int step;
    private Vector2Int gridSize;
    private Vector2Int startPos;
    private Vector2Int endPos;
    private CellType[,] grid;
    //private System.Random random;

    // --- Main Public Method ---
    // --- Main Public Method ---
    public CellType[,] Generate(GenerationParamameters_SO parameters)
    {
        // --- MODIFIED: Complete overhaul of the seeding and state management logic. ---

        // 1. Initialization
        this.p = parameters;
        this.step = p.Spacing + 1;

        // 2. Preserve Unity's current random state.
        // This is a critical best practice. It ensures our generator doesn't permanently
        // alter the random sequence for other game systems (like particle effects, AI, etc.).
        var originalRandomState = Random.state;

        // 3. Determine and apply the seed for our generation.
        int finalSeed;
        if (p.Seed != -1)
        {
            finalSeed = p.Seed; // Use the provided seed
        }
        else
        {
            // Generate a new seed. Using Unity's Random.Range ensures this is also deterministic
            // if the initial state was the same. Ticks can still have cross-platform issues.
            // A robust way is to use a combination of Ticks and a random value.
            finalSeed = (int)System.DateTime.Now.Ticks ^ Random.Range(int.MinValue, int.MaxValue);
        }

        p.currentSeed = finalSeed; // Store the seed that was actually used.
        Random.InitState(finalSeed); // Initialize Unity's generator with our definitive seed.

        // --- End of Seeding Overhaul ---

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
        grid[startPos.x, startPos.y] = CellType.Start;
        if (IsWithinBounds(endPos))
        {
            grid[endPos.x, endPos.y] = CellType.End;
        }

        // --- MODIFIED: Restore Unity's original random state. ---
        Random.state = originalRandomState;

        return grid;
    }

    // --- Core Path Generation Methods (Unchanged) ---
    // --- Core Path Generation Methods (Calls to random updated) ---
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
                // --- MODIFIED: Use Random.Range for integer values. Note the exclusive upper bound.
                score += Random.Range(0, 20);
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
                // --- MODIFIED: Use Random.Range ---
                var next = neighbors[Random.Range(0, neighbors.Count)];
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
                // --- MODIFIED: Use Random.Range ---
                var branchTo = neighbors[Random.Range(0, neighbors.Count)];
                // --- MODIFIED: Use Random.Range ---
                int branchLength = Random.Range(1, 4) * step;
                var current = cell;
                for (int j = 0; j < branchLength && IsWithinBounds(branchTo); j += step)
                {
                    if (grid[branchTo.x, branchTo.y] != CellType.Wall) break;
                    CarvePathBetween(current, branchTo);
                    current = branchTo;
                    var nextNeighbors = GetValidNeighbors(current, false);
                    if (nextNeighbors.Count == 0) break;
                    // --- MODIFIED: Use Random.Range ---
                    branchTo = nextNeighbors[Random.Range(0, nextNeighbors.Count)];
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

    // REVERTED: CarvePath now only carves a single cell.
    private void CarvePath(Vector2Int pos)
    {
        if (IsWithinBounds(pos) && grid[pos.x, pos.y] == CellType.Wall)
        {
            grid[pos.x, pos.y] = CellType.Path;
        }
    }

    private void CarvePathBetween(Vector2Int from, Vector2Int to)
    {
        Vector2Int diff = to - from;
        Vector2Int dir = new Vector2Int(Mathf.Clamp(diff.x, -1, 1), Mathf.Clamp(diff.y, -1, 1));
        int distance = (int)Mathf.Max(Mathf.Abs(diff.x), Mathf.Abs(diff.y));
        Vector2Int current = from; // Start carving from the 'from' position
        for (int i = 0; i <= distance; i++)
        {
            CarvePath(current);
            current += dir; // Move to the next cell in the line
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            // --- MODIFIED: Use Random.Range ---
            int k = Random.Range(0, i + 1); // Note: Random.Range for ints has an exclusive upper bound.
            (list[k], list[i]) = (list[i], list[k]);
        }
    }
}

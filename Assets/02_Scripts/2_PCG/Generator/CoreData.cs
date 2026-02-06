using System.Collections.Generic;
using UnityEngine;

public enum GroundType
{
    Floor,
    Ice,
    MovingPlatformH, // horizontal
    MovingPlatformV, // vertical
    PlatformSide,
    Piques,
    DoorUp,
    DoorDown,
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
    public bool isEmpty;
    public GroundType ground;
    public OverlayType overlay;
    public bool isEnd;
}

public class Grid
{
    private CellData[,] cells;

    public int Width => cells.GetLength(0);
    public int Height => cells.GetLength(1);

    public Grid(int width, int height)
    {
        cells = new CellData[width, height];
    }

    public CellData GetCell(int x, int y)
    {
        return cells[x, y];
    }

    public CellData GetCell(Vector2Int position)
    {
        return cells[position.x, position.y];
    }

    public ref CellData GetCellRef(Vector2Int pos) => ref cells[pos.x, pos.y];
    public ref CellData GetCellRef(int x, int y) => ref cells[x, y];

    public void SetCell(int x, int y, CellData cell)
    {
        cells[x, y] = cell;
    }

    public bool IsInside(int x, int y)
    => x >= 0 && y >= 0 && x < Width && y < Height;

    public bool IsInside(Vector2Int position)
        => IsInside(position.x, position.y);

    public CellData[,] Raw => cells;

    public bool TryGetEndCell(out CellData endCell)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (cells[x, y].isEnd)
                {
                    endCell = cells[x, y];
                    return true;
                }
            }
        }

        endCell = default;
        return false;
    }

    public List<CellData> GetNeighbours4(int x, int y)
    {
        var neighbours = new List<CellData>();

        if (x > 0) neighbours.Add(cells[x - 1, y]); // Left
        if (x < Width - 1) neighbours.Add(cells[x + 1, y]); // Right
        if (y > 0) neighbours.Add(cells[x, y - 1]); // Down
        if (y < Height - 1) neighbours.Add(cells[x, y + 1]); // Up

        return neighbours;
    }

    public List<CellData> GetCellsWithGround(GroundType groundType)
    {
        var result = new List<CellData>();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (cells[x, y].ground == groundType)
                    result.Add(cells[x, y]);
            }
        }

        return result;
    }

    public List<CellData> GetCellsWithOverlay(OverlayType overlayType)
    {
        var result = new List<CellData>();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (cells[x, y].overlay == overlayType)
                    result.Add(cells[x, y]);
            }
        }

        return result;
    }

    public List<CellData> GetNonEmptyCells()
    {
        var result = new List<CellData>();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (!cells[x, y].isEmpty)
                    result.Add(cells[x, y]);
            }
        }

        return result;
    }

    public List<CellData> GetEmptyCells()
    {
        var result = new List<CellData>();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (cells[x, y].isEmpty)
                    result.Add(cells[x, y]);
            }
        }

        return result;
    }

    public List<CellData> GetCellsWhere(System.Func<CellData, bool> predicate)
    {
        var result = new List<CellData>();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (predicate(cells[x, y]))
                    result.Add(cells[x, y]);
            }
        }

        return result;
    }

    public bool TryGetWalkableEndNeighbours(out List<Vector2Int> positions)
    {
        positions = new List<Vector2Int>();

        if (!TryGetEndNeighbours(out var neighbours))
            return false;

        foreach (var n in neighbours)
        {
            if (!n.cell.isEmpty && (n.cell.ground == GroundType.Floor || n.cell.ground == GroundType.Ice))
                positions.Add(n.pos);
        }

        return positions.Count > 0;
    }

    public bool TryGetEndNeighbours(out List<(Vector2Int pos, CellData cell)> neighbours)
    {
        neighbours = new List<(Vector2Int, CellData)>();

        Vector2Int endPos = new Vector2Int(-1, -1);

        // Find the End cell
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (GetCell(x, y).overlay == OverlayType.End)
                {
                    endPos = new Vector2Int(x, y);
                    break;
                }
            }
            if (endPos.x != -1)
                break;
        }

        if (endPos.x == -1)
            return false; // No End found

        // 4-way neighbours
        Vector2Int[] dirs =
            { Vector2Int.up,Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var d in dirs)
        {
            Vector2Int n = endPos + d;
            if (!IsInside(n))
                continue;

            neighbours.Add((n, GetCell(n.x, n.y)));
        }

        return true;
    }


}


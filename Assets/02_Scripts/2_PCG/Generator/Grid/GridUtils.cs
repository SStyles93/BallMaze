using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PxP.PCG
{
    public static class GridUtils
    {
        public static Vector2Int GetStartPosition(CellData[,] grid, GeneratorParameters_SO p)
        {
            int width = p.gridWidth;
            int height = p.gridHeight;

            // Always pick middle column
            int startX = width / 2;
            int startY = height - 1; // bottom row (map inverted)

            Vector2Int start = new(startX, startY);

            // Ensure the start tile is walkable
            if (IsInsideGrid(start, grid))
            {
                grid[start.x, start.y].isWall = false;
                grid[start.x, start.y].ground = GroundType.Floor;
            }

            // Carve the tile above to guarantee connectivity
            Vector2Int above = new(start.x, start.y - 1);
            if (IsInsideGrid(above, grid))
            {
                grid[above.x, above.y].isWall = false;
                grid[above.x, above.y].ground = GroundType.Floor;
            }

            return start;
        }

        public static Vector2Int ResolveEndPosition(
            GeneratorParameters_SO p,
            System.Random rng)
        {
            int maxY = Mathf.FloorToInt(
                p.gridHeight * (p.endMaxHeightPercent / 100f));

            return p.randomEnd
                ? new Vector2Int(
                    rng.Next(0, p.gridWidth),
                    rng.Next(0, Mathf.Max(0, maxY)))
                : new Vector2Int(
                    Mathf.Clamp(p.fixedEnd.x, 0, p.gridWidth),
                    Mathf.Clamp(p.fixedEnd.y, 0, maxY));
        }

        public static void MarkStartAndEnd(
            CellData[,] grid,
            Vector2Int start,
            Vector2Int end)
        {
            SetOverlay(grid, start, OverlayType.Start);
            SetOverlay(grid, end, OverlayType.End);
        }

        private static void SetOverlay(
            CellData[,] grid,
            Vector2Int pos,
            OverlayType type)
        {
            if (!IsInsideGrid(pos, grid)) return;

            ref var cell = ref grid[pos.x, pos.y];
            cell.isWall = false;
            cell.ground = GroundType.Floor;
            cell.overlay = type;
        }

        // --- Safety helpers ---

        public static int CountWallNeighbors(CellData[,] grid, int x, int y)
        {
            int count = 0;
            foreach (var d in Directions)
            {
                int nx = x + d.x;
                int ny = y + d.y;
                if (IsInsideGrid(nx, ny, grid) && grid[nx, ny].isWall)
                    count++;
            }
            return count;
        }

        public static bool CreatesOpenSquare(CellData[,] grid, int x, int y)
        {
            for (int ox = -1; ox <= 0; ox++)
                for (int oy = -1; oy <= 0; oy++)
                {
                    int open = 0;
                    for (int dx = 0; dx <= 1; dx++)
                        for (int dy = 0; dy <= 1; dy++)
                        {
                            int nx = x + ox + dx;
                            int ny = y + oy + dy;
                            if (IsInsideGrid(nx, ny, grid) &&
                                !grid[nx, ny].isWall)
                                open++;
                        }
                    if (open >= 3) return true;
                }
            return false;
        }

        public static bool IsNear(Vector2Int a, Vector2Int b)
            => Mathf.Abs(a.x - b.x) <= 1 &&
               Mathf.Abs(a.y - b.y) <= 1;

        public static bool IsInsideGrid(Vector2Int p, CellData[,] g)
            => IsInsideGrid(p.x, p.y, g);

        public static bool IsInsideGrid(int x, int y, CellData[,] g)
            => x >= 0 && y >= 0 &&
               x < g.GetLength(0) &&
               y < g.GetLength(1);

        public static void Shuffle<T>(IList<T> list, System.Random rng)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int j = rng.Next(i, list.Count);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
    }
}
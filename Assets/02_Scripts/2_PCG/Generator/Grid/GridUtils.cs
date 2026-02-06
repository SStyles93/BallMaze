using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PxP.PCG
{
    public static class GridUtils
    {
        public static Vector2Int GetStartPosition(Grid grid, GeneratorParameters_SO p)
        {
            int startX = grid.Width / 2;
            int startY = grid.Height - 1;

            Vector2Int start = new(startX, startY);

            if (grid.IsInside(start))
            {
                ref var cell = ref grid.GetCellRef(start.x, start.y);
                cell.isEmpty = false;
                cell.ground = GroundType.Floor;
            }

            Vector2Int above = new(start.x, start.y - 1);
            if (grid.IsInside(above))
            {
                ref var cell = ref grid.GetCellRef(above.x, above.y);
                cell.isEmpty = false;
                cell.ground = GroundType.Floor;
            }

            return start;
        }


        public static Vector2Int ResolveEndPosition(
            Grid grid, GeneratorParameters_SO p, System.Random rng)
        {
            int maxY = Mathf.FloorToInt(grid.Height * (p.endMaxHeightPercent / 100f));

            Vector2Int end = p.randomEnd
                ? new Vector2Int(rng.Next(0, grid.Width), rng.Next(0, Mathf.Max(0, maxY)))
                : new Vector2Int(
                    Mathf.Clamp(p.fixedEnd.x, 0, grid.Width - 1),
                    Mathf.Clamp(p.fixedEnd.y, 0, maxY)
                  );

            if (end.y + 1 < grid.Height)
            {
                ref var below = ref grid.GetCellRef(end.x, end.y + 1);
                below.isEmpty = false;
            }

            return end;
        }


        public static void MarkStartAndEnd(Grid grid, Vector2Int start, Vector2Int end)
        {
            SetOverlay(grid, start, OverlayType.Start);
            SetOverlay(grid, end, OverlayType.End);
        }

        private static void SetOverlay(Grid grid, Vector2Int pos, OverlayType type)
        {
            if (!grid.IsInside(pos)) return;

            ref var cell = ref grid.GetCellRef(pos.x, pos.y);
            cell.isEmpty = false;
            cell.ground = GroundType.Floor;
            cell.overlay = type;
        }

        // --- Safety helpers ---

        public static int CountWallNeighbors(Grid grid, int x, int y)
        {
            int count = 0;
            foreach (var d in Directions)
            {
                int nx = x + d.x;
                int ny = y + d.y;
                if (grid.IsInside(nx, ny) && grid.GetCell(nx, ny).isEmpty)
                    count++;
            }
            return count;
        }


        public static bool CreatesOpenSquare(Grid grid, int x, int y)
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
                            if (grid.IsInside(nx, ny) &&
                                !grid.GetCell(nx, ny).isEmpty)
                                open++;
                        }
                    if (open >= 3) return true;
                }
            return false;
        }

        public static bool IsNear(Vector2Int a, Vector2Int b)
            => Mathf.Abs(a.x - b.x) <= 1 &&
               Mathf.Abs(a.y - b.y) <= 1;

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
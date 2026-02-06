using System;
using System.Collections.Generic;
using UnityEngine;

namespace PxP.PCG
{
    public static class PlatformPlacer
    {
        public static void PlaceMovingPlatforms(
            Grid grid,
            GeneratorParameters_SO p,
            System.Random rng)
        {
            if (p.movingPlatformRatio <= 0) return;

            int walkableCount = CountWalkableTiles(grid);
            int maxPlatforms = Mathf.Max(
                1,
                Mathf.RoundToInt(walkableCount * p.movingPlatformRatio / 3f)
            );

            var candidates = CollectCandidates(grid);
            Shuffle(candidates, rng);

            int width = grid.Width;
            int height = grid.Height;

            bool[,] reserved = new bool[width, height];
            int placed = 0;

            foreach (var c in candidates)
            {
                if (placed >= maxPlatforms) break;
                if (IsOverlapping(reserved, c)) continue;

                ApplyPlatform(grid, reserved, c);
                placed++;
            }
        }

        // --------------------------------------------------
        // Counts walkable tiles
        // --------------------------------------------------

        private static int CountWalkableTiles(Grid grid)
        {
            int count = 0;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var cell = grid.GetCell(x, y);
                    if (!cell.isEmpty && cell.overlay == OverlayType.None)
                        count++;
                }
            }

            return count;
        }

        // --------------------------------------------------
        // Candidate collection
        // --------------------------------------------------

        private static List<(Vector2Int center, bool horizontal)> CollectCandidates(Grid grid)
        {
            var candidates = new List<(Vector2Int, bool)>();

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    if (!IsWalkable(grid, x, y)) continue;

                    // Horizontal candidate
                    if (x - 1 >= 0 && x + 1 < grid.Width &&
                        IsWalkable(grid, x - 1, y) &&
                        IsWalkable(grid, x + 1, y))
                    {
                        candidates.Add((new Vector2Int(x, y), true));
                    }

                    // Vertical candidate
                    if (y - 1 >= 0 && y + 1 < grid.Height &&
                        IsWalkable(grid, x, y - 1) &&
                        IsWalkable(grid, x, y + 1))
                    {
                        candidates.Add((new Vector2Int(x, y), false));
                    }
                }
            }

            return candidates;
        }

        // --------------------------------------------------
        // Platform application
        // --------------------------------------------------

        private static void ApplyPlatform(
            Grid grid,
            bool[,] reserved,
            (Vector2Int center, bool horizontal) c)
        {
            Vector2Int p = c.center;

            ref var centerCell = ref grid.GetCellRef(p.x, p.y);
            centerCell.isEmpty = false;
            centerCell.ground = c.horizontal
                ? GroundType.MovingPlatformH
                : GroundType.MovingPlatformV;

            if (c.horizontal)
            {
                SetSide(grid, p.x - 1, p.y);
                SetSide(grid, p.x + 1, p.y);
                Reserve(reserved,
                    p.x - 1, p.y,
                    p.x, p.y,
                    p.x + 1, p.y);
            }
            else
            {
                SetSide(grid, p.x, p.y - 1);
                SetSide(grid, p.x, p.y + 1);
                Reserve(reserved,
                    p.x, p.y - 1,
                    p.x, p.y,
                    p.x, p.y + 1);
            }
        }

        private static void SetSide(Grid grid, int x, int y)
        {
            ref var cell = ref grid.GetCellRef(x, y);

            if (cell.ground != GroundType.MovingPlatformH &&
                cell.ground != GroundType.MovingPlatformV)
            {
                cell.isEmpty = false;
                cell.ground = GroundType.PlatformSide;
            }
        }

        private static void Reserve(
            bool[,] r,
            int x1, int y1,
            int x2, int y2,
            int x3, int y3)
        {
            r[x1, y1] = r[x2, y2] = r[x3, y3] = true;
        }

        // --------------------------------------------------
        // Helpers
        // --------------------------------------------------

        private static bool IsWalkable(Grid grid, int x, int y)
        {
            var cell = grid.GetCell(x, y);
            return !cell.isEmpty && cell.overlay == OverlayType.None;
        }

        private static bool IsOverlapping(
            bool[,] reserved,
            (Vector2Int center, bool horizontal) c)
        {
            Vector2Int p = c.center;

            return c.horizontal
                ? reserved[p.x - 1, p.y] ||
                  reserved[p.x, p.y] ||
                  reserved[p.x + 1, p.y]
                : reserved[p.x, p.y - 1] ||
                  reserved[p.x, p.y] ||
                  reserved[p.x, p.y + 1];
        }

        private static void Shuffle<T>(List<T> list, System.Random rng)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int j = rng.Next(i, list.Count);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}

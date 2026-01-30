using System;
using System.Collections.Generic;
using UnityEngine;

namespace PxP.PCG
{
    public static class PlatformPlacer
    {
        public static void PlaceMovingPlatforms(CellData[,] grid, GeneratorParameters_SO p, System.Random rng)
        {

            int walkableCount = CountWalkableTiles(grid);

            int maxPlatforms = Mathf.Max(1,Mathf.RoundToInt(walkableCount * p.movingPlatformRatio / 3f));

            var candidates = CollectCandidates(grid);
            Shuffle(candidates, rng);
            
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

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

        private static int CountWalkableTiles(CellData[,] grid)
        {
            int count = 0;
            foreach (var cell in grid)
                if (!cell.isEmpty && cell.overlay == OverlayType.None)
                    count++;
            return count;
        }

        private static List<(Vector2Int center, bool horizontal)> CollectCandidates(CellData[,] grid)
        {
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);

            var candidates = new List<(Vector2Int, bool)>();

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!IsWalkable(grid, x, y)) continue;

                    // Horizontal candidate
                    if (x - 1 >= 0 && x + 1 < w &&
                        IsWalkable(grid, x - 1, y) && IsWalkable(grid, x + 1, y))
                    {
                        candidates.Add((new Vector2Int(x, y), true));
                    }

                    // Vertical candidate
                    if (y - 1 >= 0 && y + 1 < h &&
                        IsWalkable(grid, x, y - 1) && IsWalkable(grid, x, y + 1))
                    {
                        candidates.Add((new Vector2Int(x, y), false));
                    }
                }
            }

            return candidates;
        }

        // --------------------------------------------------

        private static void ApplyPlatform(CellData[,] grid, bool[,] reserved, (Vector2Int center, bool horizontal) c)
        {
            var p = c.center;
            ref CellData centerCell = ref grid[p.x, p.y];

            centerCell.isEmpty = false;
            centerCell.ground = c.horizontal ? GroundType.MovingPlatformH : GroundType.MovingPlatformV;

            if (c.horizontal)
            {
                SetSide(grid, p.x - 1, p.y);
                SetSide(grid, p.x + 1, p.y);
                Reserve(reserved, p.x - 1, p.y, p.x, p.y, p.x + 1, p.y);
            }
            else
            {
                SetSide(grid, p.x, p.y - 1);
                SetSide(grid, p.x, p.y + 1);
                Reserve(reserved, p.x, p.y - 1, p.x, p.y, p.x, p.y + 1);
            }
        }

        private static void SetSide(CellData[,] grid, int x, int y)
        {
            ref CellData cell = ref grid[x, y];
            if (cell.ground != GroundType.MovingPlatformH && cell.ground != GroundType.MovingPlatformV)
            {
                cell.isEmpty = false;
                cell.ground = GroundType.PlatformSide;
            }
        }

        private static void Reserve(bool[,] r, int x1, int y1, int x2, int y2, int x3, int y3)
        {
            r[x1, y1] = r[x2, y2] = r[x3, y3] = true;
        }

        // --------------------------------------------------

        private static bool IsWalkable(CellData[,] grid, int x, int y)
        {
            return !grid[x, y].isEmpty && grid[x, y].overlay == OverlayType.None;
        }

        private static bool IsOverlapping(bool[,] reserved, (Vector2Int center, bool horizontal) c)
        {
            var p = c.center;

            if (c.horizontal)
                return reserved[p.x - 1, p.y] || reserved[p.x, p.y] || reserved[p.x + 1, p.y];
            else
                return reserved[p.x, p.y - 1] || reserved[p.x, p.y] || reserved[p.x, p.y + 1];
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

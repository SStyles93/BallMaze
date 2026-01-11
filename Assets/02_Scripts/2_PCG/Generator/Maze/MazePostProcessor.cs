using System.Collections.Generic;
using UnityEngine;

namespace PxP.PCG
{
    public static class MazePostProcessor
    {
        public static void AddSafeLoops(CellData[,] grid, float chance, System.Random rng)
        {
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);

            for (int x = 1; x < w - 1; x++)
                for (int y = 1; y < h - 1; y++)
                {
                    if (!grid[x, y].isWall) continue;
                    if (rng.NextDouble() > chance) continue;

                    if (!CanSafelyCarve(grid, x, y))
                        continue;

                    grid[x, y].isWall = false;
                }
        }

        public static void CarveEmptyTiles( CellData[,] grid,GeneratorParameters_SO p, 
            Vector2Int start, Vector2Int end, System.Random rng)
        {
            if (p.emptyRatio <= 0f) return;

            var candidates = new List<Vector2Int>();

            for (int x = 1; x < grid.GetLength(0) - 1; x++)
                for (int y = 1; y < grid.GetLength(1) - 1; y++)
                {
                    if (grid[x, y].isWall &&
                        !GridUtils.IsNear(new Vector2Int(x, y), start) &&
                        !GridUtils.IsNear(new Vector2Int(x, y), end))
                    {
                        candidates.Add(new Vector2Int(x, y));
                    }
                }

            GridUtils.Shuffle(candidates, rng);

            int target = Mathf.RoundToInt(candidates.Count * p.emptyRatio);

            foreach (var pos in candidates)
            {
                if (target-- <= 0) break;
                if (!CanSafelyCarve(grid, pos.x, pos.y)) continue;

                grid[pos.x, pos.y].isWall = false;
            }
        }

        private static bool CanSafelyCarve(CellData[,] grid, int x, int y)
        {
            return GridUtils.CountWallNeighbors(grid, x, y) >= 2
                && !GridUtils.CreatesOpenSquare(grid, x, y);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace PxP.PCG
{
    public static class MazePostProcessor
    {

        public static void CarveEmptyTiles(CellData[,] grid, GeneratorParameters_SO p,
    Vector2Int start, Vector2Int end, System.Random rng)
        {
            if (p.emptyRatio <= 0f) return;

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            var candidates = new List<Vector2Int>();

            // Collect all solid ground tiles that are eligible to become holes
            for (int x = 0; x < width - 1; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    ref var cell = ref grid[x, y];

                    if (!cell.isEmpty && cell.overlay == OverlayType.None &&
                        !GridUtils.IsNear(new Vector2Int(x, y), start) &&
                        !GridUtils.IsNear(new Vector2Int(x, y), end))
                    {
                        candidates.Add(new Vector2Int(x, y));
                    }
                }
            }

            GridUtils.Shuffle(candidates, rng);

            int targetCount = Mathf.RoundToInt(candidates.Count * p.emptyRatio);

            foreach (var pos in candidates)
            {
                if (targetCount-- <= 0) break;

                // Count solid neighbours (up, down, left, right)
                int solidNeighbours = 0;
                Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

                foreach (var d in dirs)
                {
                    Vector2Int n = pos + d;
                    if (n.x >= 0 && n.x < width && n.y >= 0 && n.y < height &&
                        !grid[n.x, n.y].isEmpty)
                    {
                        solidNeighbours++;
                    }
                }

                // Only carve hole if at least 2 solid neighbours remain
                if (solidNeighbours >= 2)
                {
                    grid[pos.x, pos.y].isEmpty = true;
                }
            }
        }
    }
}
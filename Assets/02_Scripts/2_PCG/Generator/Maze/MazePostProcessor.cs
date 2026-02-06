using System.Collections.Generic;
using UnityEngine;

namespace PxP.PCG
{
    public static class MazePostProcessor
    {
        public static void CarveEmptyTiles(
            Grid grid,
            GeneratorParameters_SO p,
            Vector2Int start,
            Vector2Int end,
            System.Random rng)
        {
            float emptyRatio = p.emptyRatio;

            int width = grid.Width;
            int height = grid.Height;

            var candidates = new List<Vector2Int>();

            // Collect all solid ground tiles that are eligible to become holes
            for (int x = 0; x < width - 1; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    ref var cell = ref grid.GetCellRef(x, y);

                    if (!cell.isEmpty &&
                        cell.overlay == OverlayType.None &&
                        !GridUtils.IsNear(new Vector2Int(x, y), start) &&
                        !GridUtils.IsNear(new Vector2Int(x, y), end))
                    {
                        candidates.Add(new Vector2Int(x, y));
                    }
                }
            }

            GridUtils.Shuffle(candidates, rng);
            int targetCount = Mathf.RoundToInt(candidates.Count * emptyRatio);

            foreach (var pos in candidates)
            {
                if (targetCount-- <= 0)
                    break;

                int solidNeighbours = 0;

                foreach (var d in Directions4)
                {
                    Vector2Int n = pos + d;

                    if (grid.IsInside(n) && !grid.GetCell(n.x, n.y).isEmpty)
                    {
                        solidNeighbours++;
                    }
                }

                // Only carve hole if at least 2 solid neighbours remain
                if (solidNeighbours >= 2)
                {
                    ref var cell = ref grid.GetCellRef(pos.x, pos.y);
                    cell.isEmpty = true;
                }
            }
        }

        private static readonly Vector2Int[] Directions4 =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
    }
}

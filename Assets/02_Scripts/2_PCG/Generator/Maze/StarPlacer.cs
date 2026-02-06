using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PxP.PCG
{
    public static class StarPlacer
    {

        public static void PlaceStars(Grid grid, GeneratorParameters_SO p, HashSet<Vector2Int> walkable,
            Vector2Int start, Vector2Int end, System.Random rng)
        {
            var placed = new List<Vector2Int>();
            var candidates = walkable
                .Where(c => c != start && c != end)
                .ToList();

            GridUtils.Shuffle(candidates, rng);

            // First pass: try respecting minStarDistance
            foreach (var pos in candidates)
            {
                if (placed.Count >= p.starCount) break;
                if (grid.GetCell(pos.x, pos.y).overlay != OverlayType.None) continue;

                if (placed.Any(s => Vector2Int.Distance(s, pos) < p.minStarDistance))
                    continue;

                grid.GetCellRef(pos.x, pos.y).overlay = OverlayType.Star;
                placed.Add(pos);
            }

            // Second pass: relax minStarDistance if needed
            if (placed.Count < p.starCount)
            {
                foreach (var pos in candidates)
                {
                    if (placed.Count >= p.starCount) break;
                    if (grid.GetCellRef(pos.x, pos.y).overlay != OverlayType.None) continue;
                    if (placed.Contains(pos)) continue;

                    grid.GetCellRef(pos.x, pos.y).overlay = OverlayType.Star;
                    placed.Add(pos);
                }
            }
        }

    }
}
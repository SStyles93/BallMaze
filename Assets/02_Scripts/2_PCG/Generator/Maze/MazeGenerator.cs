using System;
using System.Collections.Generic;
using UnityEngine;

namespace PxP.PCG
{
    public static class MazeGenerator
    {
        public static HashSet<Vector2Int> GenerateKruskalMaze(int width, int height, System.Random rng)
        {
            var cells = new List<Vector2Int>();
            var edges = new List<Edge>();

            for (int x = 0; x < width; x += 2)
                for (int y = 0; y < height; y += 2)
                    cells.Add(new Vector2Int(x, y));

            foreach (var c in cells)
            {
                TryAddEdge(c, Vector2Int.right * 2);
                TryAddEdge(c, Vector2Int.up * 2);
            }

            void TryAddEdge(Vector2Int a, Vector2Int delta)
            {
                Vector2Int b = a + delta;

                if (b.x < 0 || b.x >= width ||
                    b.y < 0 || b.y >= height)
                    return;

                edges.Add(new Edge { a = a, b = b });
            }

            GridUtils.Shuffle(edges, rng);

            var uf = new UnionFind<Vector2Int>(cells);
            var carved = new HashSet<Vector2Int>();

            foreach (var e in edges)
            {
                if (!uf.Union(e.a, e.b)) continue;

                carved.Add(e.a);
                carved.Add(e.b);
                carved.Add((e.a + e.b) / 2);
            }

            return carved;
        }

        public static void ApplyMaze(CellData[,] grid, HashSet<Vector2Int> carved, GeneratorParameters_SO p, System.Random rng)
        {
            foreach (var pos in carved)
            {
                if (!GridUtils.IsInsideGrid(pos, grid))
                    continue;

                ref var cell = ref grid[pos.x, pos.y];
                cell.isEmpty = false;

                // Floor ratio is always "whatever is left"
                float floorRatio = 1;
                float totalModifiers = p.iceRatio + p.piquesRatio + p.doorDownRatio + p.doorUpRatio;

                // Ensure modifiers don’t exceed 1
                float clampedTotal = Math.Min(totalModifiers, floorRatio);

                // Compute actual modifier probabilities scaled to total left for modifiers
                float scale = floorRatio > 0 ? clampedTotal / totalModifiers : 0f;

                float iceProb = p.iceRatio * scale;
                float piquesProb = p.piquesRatio * scale;
                float doorDownProb = p.doorDownRatio * scale;
                float doorUpProb = p.doorUpRatio * scale;

                // Floor gets the leftover
                float floorProb = floorRatio - (iceProb + piquesProb + doorDownProb + doorUpProb);

                // Roll
                double roll = rng.NextDouble();

                if (roll < iceProb)
                    cell.ground = GroundType.Ice;
                else if (roll < iceProb + piquesProb)
                    cell.ground = GroundType.Piques;
                else if (roll < iceProb + piquesProb + doorDownProb)
                    cell.ground = GroundType.DoorDown;
                else if (roll < iceProb + piquesProb + doorDownProb + doorUpProb)
                    cell.ground = GroundType.DoorUp;
                else
                    cell.ground = GroundType.Floor;

                // **************************
                // ADD ANY MODIFIER TYPE HERE
                // **************************
            }
        }


        private struct Edge
        {
            public Vector2Int a;
            public Vector2Int b;
        }
    }
}
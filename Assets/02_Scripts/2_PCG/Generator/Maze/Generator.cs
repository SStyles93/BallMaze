using UnityEngine;

namespace PxP.PCG
{
    public static class Generator
    {
        public static CellData[,] GenerateMaze(
            GeneratorParameters_SO p,
            out int usedSeed)
        {
            usedSeed = p.inputSeed == -1
                ? Random.Range(int.MinValue, int.MaxValue)
                : p.inputSeed;

            var rng = new System.Random(usedSeed);

            var grid = GridFactory.CreateWallGrid(p.gridWidth, p.gridHeight);

            // 1. Perfect maze
            var carved = MazeGenerator.GenerateKruskalMaze(p.gridWidth, p.gridHeight, rng);
            MazeGenerator.ApplyMaze(grid, carved, p, rng);

            // 2. Controlled loops
            MazePostProcessor.AddSafeLoops(grid, p.loopChance, rng);

            // 3. Start / End
            var start = GridUtils.GetStartPosition(grid, p);
            var end = GridUtils.ResolveEndPosition(p, rng);
            GridUtils.MarkStartAndEnd(grid, start, end);

            // 4. Stars & connections
            StarPlacer.PlaceStars(grid, p, carved, start, end, rng);

            // 5. Optional wall erosion
            MazePostProcessor.CarveEmptyTiles(grid, p,start, end, rng);

            // 6. Platforms
            PlatformPlacer.PlaceMovingPlatforms(grid,p, rng);

            return grid;
        }
    }
}
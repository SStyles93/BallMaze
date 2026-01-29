
namespace PxP.PCG
{
    public static class GridFactory
    {
        public static CellData[,] CreateWallGrid(int width, int height)
        {
            var grid = new CellData[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = new CellData
                    {
                        isEmpty = true,
                        ground = GroundType.Floor,
                        overlay = OverlayType.NONE
                    };
                }

            return grid;
        }
    }
}

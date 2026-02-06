
namespace PxP.PCG
{
    public static class GridFactory
    {
        public static Grid CreateWallGrid(int width, int height)
        {
            Grid grid = new Grid(width, height);

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    grid.SetCell(x, y, new CellData
                    {
                        isEmpty = true,
                        ground = GroundType.Floor,
                        overlay = OverlayType.None,
                        isEnd = false
                    });
                }

            return grid;
        }
    }
}

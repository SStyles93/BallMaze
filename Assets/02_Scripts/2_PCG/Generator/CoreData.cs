public enum GroundType
{
    Floor,
    Ice,
    MovingPlatformH, // horizontal
    MovingPlatformV, // vertical
    PlatformSide
}

public enum OverlayType
{
    None,
    Start,
    End,
    Star
}

[System.Serializable]
public struct CellData
{
    public bool isWall;
    public GroundType ground;
    public OverlayType overlay;
}


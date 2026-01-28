public enum GroundType
{
    Floor,
    Ice,
    MovingPlatformH, // horizontal
    MovingPlatformV, // vertical
    PlatformSide,
    Piques
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
    public bool isEmpty;
    public GroundType ground;
    public OverlayType overlay;
}


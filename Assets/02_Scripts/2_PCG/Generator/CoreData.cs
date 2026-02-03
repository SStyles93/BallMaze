public enum GroundType
{
    Floor,
    Ice,
    MovingPlatformH, // horizontal
    MovingPlatformV, // vertical
    PlatformSide,
    Piques,
    DoorUp,
    DoorDown,
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
    public bool isEnd;
}


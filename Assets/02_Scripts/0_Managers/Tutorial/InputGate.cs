using System;

[Flags]
public enum AllowedInput
{
    None = 0,
    Move = 1 << 0,
    Jump = 1 << 1,
    Touch = 1 << 2,
    Swipe = 1 << 3,
    Tap = 1 << 4,
    All = ~0
}
public static class InputGate
{
    public static AllowedInput Allowed = AllowedInput.All;
}



[Flags]
public enum AllowedMovement
{
    None = 0,
    Forward = 1 << 0,
    Backward = 1 << 1,
    Left = 1 << 2,
    Right = 1 << 3,
    All = Forward | Backward | Left | Right
}

public static class MovementGate
{
    public static AllowedMovement Allowed = AllowedMovement.All;
}

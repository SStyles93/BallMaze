using System;

[Serializable]
public struct LevelTelemetryEvent
{
    public int levelIndex;
    public int cycleIndex;
    public string archetypeName;
    public string dominantModifier;
    public string result;   // "success" / "fail" / "quit"
    public int attemptNumber;
    public float duration;  // in seconds

    public LevelTelemetryEvent(int levelIndex, int cycleIndex, string archetypeName,
                               string dominantModifier, string result, int attemptNumber, float duration)
    {
        this.levelIndex = levelIndex;
        this.cycleIndex = cycleIndex;
        this.archetypeName = archetypeName;
        this.dominantModifier = dominantModifier;
        this.result = result;
        this.attemptNumber = attemptNumber;
        this.duration = duration;
    }
}

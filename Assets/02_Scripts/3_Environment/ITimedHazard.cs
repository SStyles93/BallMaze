public interface ITimedHazard
{
    /// <summary>
    /// phase01 [0,1]
    /// </summary>
    /// <param name="phase01"></param>
    void SetState(bool isInverted);

    /// <summary>
    /// Full cycle duration in seconds
    /// </summary>
    float CycleDuration { get; }

    /// <summary>
    /// Is the tile safe at a given local time?
    /// </summary>
    /// <param name="timeInCycle"></param>
    /// <returns></returns>
    bool IsSafe();
}

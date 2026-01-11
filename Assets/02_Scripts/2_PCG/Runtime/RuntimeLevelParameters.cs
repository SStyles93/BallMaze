using System;
using UnityEngine;

[Serializable]
public struct RuntimeLevelParameters
{
    public int width;
    public int height;
    public int minStarDistance;
    public float emptyRatio;
    public float iceRatio;
    public float movingPlatformRatio;
}

public static class RuntimeLevelProgression
{
    public static RuntimeLevelParameters GetParametersForLevel(
        int levelIndex,
        int levelsPerCycle = 30
    )
    {
        RuntimeLevelParameters p = new RuntimeLevelParameters();

        // -------------------------
        // BASE VALUES
        // -------------------------
        int minWidth = 3;
        int maxWidth = 10;
        int minHeight = 10;
        int maxHeight = 20;
        int minStarDistance = 3;
        int maxStarDistance = 10;

        // -------------------------
        // CYCLE PROGRESSION
        // -------------------------
        int cycleIndex = levelIndex / levelsPerCycle;      // 0,1,2,3...
        int cycleLevel = levelIndex % levelsPerCycle;     // position inside cycle
        float cycleT = (float)cycleLevel / (levelsPerCycle - 1); // [0..1]

        // -------------------------
        // PHASE TIMING (normalized)
        // -------------------------
        float phase0End = 0.20f;
        float phase1End = 0.40f;
        float phase2End = 0.60f;
        float phase3End = 0.80f;

        int width = minWidth;
        int height = minHeight;
        int starDistance = minStarDistance;

        // Phase progression for width/height/starDistance
        if (cycleT <= phase0End)
        {
            float t = cycleT / phase0End;
            width = Mathf.RoundToInt(Mathf.Lerp(minWidth, maxWidth, t));
        }
        else if (cycleT <= phase1End)
        {
            width = maxWidth;
        }
        else if (cycleT <= phase2End)
        {
            float t = (cycleT - phase1End) / (phase2End - phase1End);
            width = Mathf.RoundToInt(Mathf.Lerp(maxWidth, maxWidth + 2, t));
            height = Mathf.RoundToInt(Mathf.Lerp(minHeight, maxHeight, t));
        }
        else if (cycleT <= phase3End)
        {
            width = maxWidth + 2;
            height = maxHeight;
        }
        else
        {
            float t = (cycleT - phase3End) / (1f - phase3End);
            starDistance = Mathf.RoundToInt(Mathf.Lerp(minStarDistance, maxStarDistance, t));
        }

        // -------------------------
        // ENVIRONMENTAL DIFFICULTY
        // -------------------------
        float emptyRatio = 0f;
        float iceRatio = 0f;
        float movingPlatformRatio = 0f;

        if (cycleIndex >= 1)
            emptyRatio = Mathf.Clamp01(cycleT);

        if (cycleIndex >= 2)
            iceRatio = Mathf.Clamp01(cycleT);

        if (cycleIndex >= 3)
        {
            emptyRatio = Mathf.Clamp01(cycleT);
            iceRatio = Mathf.Clamp01(cycleT);
        }

        if (cycleIndex >= 4)
            movingPlatformRatio = Mathf.Clamp01(0.5f * cycleT);

        // -------------------------
        // OUTPUT
        // -------------------------
        p.width = width;
        p.height = height;
        p.minStarDistance = Mathf.Clamp(starDistance, 1, 10);
        p.emptyRatio = emptyRatio;
        p.iceRatio = iceRatio;
        p.movingPlatformRatio = movingPlatformRatio;

        return p;
    }
}

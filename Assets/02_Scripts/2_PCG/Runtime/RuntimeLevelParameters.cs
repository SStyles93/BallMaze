using System;
using UnityEngine;

[Serializable]
public struct RuntimeLevelParameters
{
    public int width;
    public int height;
    public int curvePercent;
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
        int minCurve = 0;
        int maxCurve = 30;
        int minStarDistance = 3;
        int maxStarDistance = 10;

        // -------------------------
        // CYCLE PROGRESSION
        // -------------------------
        int cycleIndex = levelIndex / levelsPerCycle;      // 0,1,2,3...
        int cycleLevel = levelIndex % levelsPerCycle;      // position inside cycle
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
        int curve = maxCurve;
        int starDistance = minStarDistance;

        if (cycleT <= phase0End)
        {
            float t = cycleT / phase0End;
            width = Mathf.RoundToInt(Mathf.Lerp(minWidth, maxWidth, t));
        }
        else if (cycleT <= phase1End)
        {
            float t = (cycleT - phase0End) / (phase1End - phase0End);
            width = maxWidth;
            curve = Mathf.RoundToInt(Mathf.Lerp(maxCurve, minCurve, t));
        }
        else if (cycleT <= phase2End)
        {
            float t = (cycleT - phase1End) / (phase2End - phase1End);
            width = Mathf.RoundToInt(Mathf.Lerp(maxWidth, maxWidth + 2, t));
            height = Mathf.RoundToInt(Mathf.Lerp(minHeight, maxHeight, t));
            curve = maxCurve;
        }
        else if (cycleT <= phase3End)
        {
            float t = (cycleT - phase2End) / (phase3End - phase2End);
            curve = Mathf.RoundToInt(Mathf.Lerp(maxCurve, minCurve, t));
        }
        else
        {
            float t = (cycleT - phase3End) / (1f - phase3End);
            starDistance = Mathf.RoundToInt(Mathf.Lerp(minStarDistance, maxStarDistance, t));
            curve = minCurve;
        }

        // -------------------------
        // ENVIRONMENTAL DIFFICULTY
        // -------------------------

        float emptyRatio = 0f;
        float iceRatio = 0f;
        float movingPlatformRatio = 0f;

        // Cycle 1 → 2 : Holes
        if (cycleIndex >= 1)
        {
            emptyRatio = Mathf.Clamp01(cycleT);
        }

        // Cycle 2 → 3 : Ice
        if (cycleIndex >= 2)
        {
            iceRatio = Mathf.Clamp01(cycleT);
        }

        // Cycle 3 → 4: Both keep increasing but capped
        if (cycleIndex >= 3)
        {
            // Gradually increase from 0 → 100% during the cycle
            emptyRatio = Mathf.Clamp01(1f * cycleT);
            iceRatio = Mathf.Clamp01(1f * cycleT);
        }

        // Cycle 4 → 5: Moving Platforms
        if (cycleIndex >= 4)
        {
            // Gradually increase from 0 → 50% during the cycle
            movingPlatformRatio = Mathf.Clamp01(0.5f * cycleT);
        }

        // -------------------------
        // OUTPUT
        // -------------------------
        p.width = width;
        p.height = height;
        p.curvePercent = curve;
        p.minStarDistance = Mathf.Clamp(starDistance, 1, 10);
        p.emptyRatio = emptyRatio;
        p.iceRatio = iceRatio;
        p.movingPlatformRatio = movingPlatformRatio;

        return p;
    }

}
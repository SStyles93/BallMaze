using System;
using UnityEngine;

[Serializable]
public struct RuntimeLevelParameters
{
    public int width;
    public int height;
    public int curvePercent;
    public int minStarDistance;
    public float iceRatio;
}

public static class RuntimeLevelProgression
{
    public static RuntimeLevelParameters GetParametersForLevel(
        int levelIndex,
        int levelsPerCycle = 30 // total levels in one cycle
    )
    {
        RuntimeLevelParameters p = new RuntimeLevelParameters();

        // -------------------------
        // INITIAL VALUES
        // -------------------------
        int minWidth = 3;
        int maxWidth = 10;
        int minHeight = 10;
        int maxHeight = 20;
        int minCurve = 0;
        int maxCurve = 30;
        int minStarDistance = 2;
        int maxStarDistance = 10;

        float iceRatio = 0f;

        // Current position in cycle (0 → levelsPerCycle-1)
        int cycleLevel = levelIndex % levelsPerCycle;
        float t = (float)cycleLevel / levelsPerCycle; // normalized [0,1] in cycle

        // --- PHASES as percentage of the cycle ---
        // Phase 0: width growth  (0% → 25% of cycle)
        float phase0End = 0.25f;
        // Phase 1: curve decrease (25% → 40%)
        float phase1End = 0.40f;
        // Phase 2: width+height increase (40% → 60%)
        float phase2End = 0.60f;
        // Phase 3: curve decrease again (60% → 80%)
        float phase3End = 0.80f;
        // Phase 4: star distance increase (80% → 100%)
        float phase4End = 1f;

        int width = minWidth;
        int height = minHeight;
        int curve = maxCurve;
        int starDistance = minStarDistance;

        if (t <= phase0End)
        {
            // Phase 0 - width grows from minWidth → maxWidth
            float localT = t / phase0End;
            width = Mathf.RoundToInt(Mathf.Lerp(minWidth, maxWidth, localT));
        }
        else if (t <= phase1End)
        {
            // Phase 1 - curve decreases maxCurve → minCurve
            float localT = (t - phase0End) / (phase1End - phase0End);
            width = maxWidth;
            starDistance = 4;
            curve = Mathf.RoundToInt(Mathf.Lerp(maxCurve, minCurve, localT));
        }
        else if (t <= phase2End)
        {
            // Phase 2 - width+height increase
            float localT = (t - phase1End) / (phase2End - phase1End);
            width = Mathf.RoundToInt(Mathf.Lerp(maxWidth, maxWidth + 2, localT));
            height = Mathf.RoundToInt(Mathf.Lerp(minHeight, maxHeight, localT));
            curve = maxCurve;
        }
        else if (t <= phase3End)
        {
            // Phase 3 - curve decrease again
            float localT = (t - phase2End) / (phase3End - phase2End);
            width = maxWidth + 2;
            height = maxHeight;
            curve = Mathf.RoundToInt(Mathf.Lerp(maxCurve, minCurve, localT));
        }
        else
        {
            // Phase 4 - star distance increase
            float localT = (t - phase3End) / (phase4End - phase3End);
            width = maxWidth + 2;
            height = maxHeight;
            curve = minCurve;
            starDistance = Mathf.RoundToInt(Mathf.Lerp(4, maxStarDistance, localT));
        }

        // --- Ice Ratio ---
        int cyclesCompleted = levelIndex / levelsPerCycle;
        if (cyclesCompleted > 0)
        {
            iceRatio = Mathf.Clamp01(0.05f * cyclesCompleted); // 5% per completed cycle
        }

        p.width = width;
        p.height = height;
        p.curvePercent = Mathf.Clamp(curve, 0, 100);
        p.minStarDistance = Mathf.Clamp(starDistance, 1, 10);
        p.iceRatio = iceRatio;

        return p;
    }
}
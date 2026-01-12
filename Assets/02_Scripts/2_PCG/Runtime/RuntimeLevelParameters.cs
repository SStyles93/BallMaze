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
    // -------------------------
    // DESIGN CONSTANTS
    // -------------------------

    const float MAX_EMPTY = 0.5f;
    const float MAX_ICE = 1.0f;
    const float MAX_MOVING = 0.5f;

    const float DOMINANT_WEIGHT = 0.7f;
    const float SECONDARY_WEIGHT = 0.3f;

    enum LevelArchetype
    {
        Recovery,
        Precision,              // Empty dominant
        Slippery,               // Ice dominant
        Timing,                 // Moving dominant
        PrecisionSlippery,      // Empty + Ice
        PrecisionTiming,        // Empty + Moving
        SlipperyTiming          // Ice + Moving
    }

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
        int cycleIndex = levelIndex / levelsPerCycle;
        int cycleLevel = levelIndex % levelsPerCycle;
        float cycleT = (float)cycleLevel / (levelsPerCycle - 1);

        // -------------------------
        // PHASE TIMING
        // -------------------------
        float phase0End = 0.20f;
        float phase1End = 0.40f;
        float phase2End = 0.60f;
        float phase3End = 0.80f;

        int width = minWidth;
        int height = minHeight;
        int starDistance = minStarDistance;

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

        bool isRecoveryLevel = IsRecoveryLevel(cycleIndex, cycleLevel);
        LevelArchetype archetype = SelectArchetype(cycleIndex, cycleLevel, isRecoveryLevel);

        float emptyRatio;
        float iceRatio;
        float movingPlatformRatio;

        ApplyArchetype(
            archetype,
            cycleT,
            out emptyRatio,
            out iceRatio,
            out movingPlatformRatio
        );

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

    // -------------------------
    // RECOVERY LOGIC
    // -------------------------

    static bool IsRecoveryLevel(int cycleIndex, int cycleLevel)
    {
        int recoveryFrequency = Mathf.Clamp(6 - cycleIndex, 3, 6);
        return (cycleLevel % recoveryFrequency) == recoveryFrequency - 1;
    }

    // -------------------------
    // ARCHETYPE SELECTION
    // -------------------------

    static LevelArchetype SelectArchetype(
        int cycleIndex,
        int cycleLevel,
        bool isRecovery
    )
    {
        if (isRecovery)
            return LevelArchetype.Recovery;

        bool emptyUnlocked = cycleIndex >= 1;
        bool iceUnlocked = cycleIndex >= 2;
        bool movingUnlocked = cycleIndex >= 4;

        // Early game: single-modifier focus
        if (cycleIndex < 3)
        {
            if (emptyUnlocked)
                return LevelArchetype.Precision;

            return LevelArchetype.Recovery;
        }

        // Mid / late game structured rotation
        int pattern = cycleLevel % 3;

        if (pattern == 0 && emptyUnlocked && iceUnlocked)
            return LevelArchetype.PrecisionSlippery;

        if (pattern == 1 && iceUnlocked && movingUnlocked)
            return LevelArchetype.SlipperyTiming;

        if (emptyUnlocked && movingUnlocked)
            return LevelArchetype.PrecisionTiming;

        return LevelArchetype.Precision;
    }

    // -------------------------
    // ARCHETYPE APPLICATION
    // -------------------------

    static void ApplyArchetype(
        LevelArchetype archetype,
        float t,
        out float empty,
        out float ice,
        out float moving
    )
    {
        empty = ice = moving = 0f;

        switch (archetype)
        {
            case LevelArchetype.Precision:
                empty = MAX_EMPTY * DOMINANT_WEIGHT * t;
                break;

            case LevelArchetype.Slippery:
                ice = MAX_ICE * DOMINANT_WEIGHT * t;
                break;

            case LevelArchetype.Timing:
                moving = MAX_MOVING * DOMINANT_WEIGHT * t;
                break;

            case LevelArchetype.PrecisionSlippery:
                empty = MAX_EMPTY * DOMINANT_WEIGHT * t;
                ice = MAX_ICE * SECONDARY_WEIGHT * t;
                break;

            case LevelArchetype.PrecisionTiming:
                empty = MAX_EMPTY * DOMINANT_WEIGHT * t;
                moving = MAX_MOVING * SECONDARY_WEIGHT * t;
                break;

            case LevelArchetype.SlipperyTiming:
                ice = MAX_ICE * DOMINANT_WEIGHT * t;
                moving = MAX_MOVING * SECONDARY_WEIGHT * t;
                break;

            case LevelArchetype.Recovery:
            default:
                break;
        }
    }
}

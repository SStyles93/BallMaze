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

    // -------------------------
    // ADAPTIVE DIFFICULTY
    // -------------------------

    /// <summary>
    /// Calculates a difficulty multiplier based on lives lost and number of previous failures.
    /// Lower multiplier = easier level.
    /// </summary>
    /// <param name="livesLost">Number of lives lost in the current attempt</param>
    /// <param name="failedTimes">Number of previous failed attempts for this level</param>
    /// <returns>Multiplier between 0.5f and 1.0f</returns>
    static float GetDifficultyMultiplier(int livesLost, int failedTimes)
    {
        // Base multiplier from lives lost
        float multiplier = 1f;

        if (livesLost >= 3) multiplier = 0.5f;   // big relief if game over
        else if (livesLost == 2) multiplier = 0.7f; // moderate relief
        else if (livesLost == 1) multiplier = 0.85f; // small relief

        // Apply additional reduction based on previous failures
        // Use diminishing returns: each failure reduces by 5% max 3 failures
        int cappedFailures = Mathf.Min(failedTimes, 3);
        multiplier *= 1f - (0.05f * cappedFailures);

        // Clamp final multiplier to avoid too easy
        return Mathf.Clamp(multiplier, 0.5f, 1f);
    }


    // -------------------------
    // MAIN FUNCTION
    // -------------------------

    public static RuntimeLevelParameters GetParametersForLevel(
        int levelIndex,
        LevelArchetypeDatabase_SO archetypeDatabase,
        int levelsPerCycle = 30,
        int livesLostThisLevel = 0, int failedTimes = 0)
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

        bool isRecovery = IsRecoveryLevel(cycleIndex, cycleLevel);

        LevelArchetypeData_SO archetype =
            SelectArchetype(archetypeDatabase, cycleIndex, cycleLevel, isRecovery);

        ApplyArchetypeData(
            archetype,
            cycleT,
            out float emptyRatio,
            out float iceRatio,
            out float movingPlatformRatio
        );

        // -------------------------
        // ADAPTIVE DIFFICULTY
        // -------------------------

        float multiplier = GetDifficultyMultiplier(livesLostThisLevel, failedTimes);

        emptyRatio *= multiplier;
        iceRatio *= multiplier;
        movingPlatformRatio *= multiplier;

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

    static LevelArchetypeData_SO SelectArchetype(
        LevelArchetypeDatabase_SO db,
        int cycleIndex, int cycleLevel, bool isRecovery)
    {
        if (db == null)
        {
            Debug.LogError("LevelArchetypeDatabase is NULL");
            return null;
        }

        if (isRecovery)
            return db.recovery;

        bool emptyUnlocked = cycleIndex >= 1;
        bool iceUnlocked = cycleIndex >= 2;
        bool movingUnlocked = cycleIndex >= 4;

        if (cycleIndex < 3 && emptyUnlocked)
            return db.precision;

        int pattern = cycleLevel % 3;

        if (pattern == 0 && emptyUnlocked && iceUnlocked)
            return db.precisionSlippery;

        if (pattern == 1 && iceUnlocked && movingUnlocked)
            return db.slipperyTiming;

        if (emptyUnlocked && movingUnlocked)
            return db.precisionTiming;

        return db.precision;
    }

    // -------------------------
    // ARCHETYPE APPLICATION
    // -------------------------

    static void ApplyArchetypeData(
        LevelArchetypeData_SO data, float t,
        out float empty, out float ice, out float moving)
    {
        empty = ice = moving = 0f;

        if (data == null || data.modifiers == null)
            return;

        foreach (var mod in data.modifiers)
        {
            float scaled = Mathf.Clamp01(mod.weight * t);

            switch (mod.type)
            {
                case ModifierType.Empty:
                    empty = Mathf.Min(MAX_EMPTY, scaled * MAX_EMPTY);
                    break;

                case ModifierType.Ice:
                    ice = Mathf.Min(MAX_ICE, scaled * MAX_ICE);
                    break;

                case ModifierType.Moving:
                    moving = Mathf.Min(MAX_MOVING, scaled * MAX_MOVING);
                    break;
            }
        }
    }
}

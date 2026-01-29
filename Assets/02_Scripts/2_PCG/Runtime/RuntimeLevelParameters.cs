using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct RuntimeLevelParameters
{
    public int width;
    public int height;
    public int minStarDistance;

    // Flexible dictionary for any tile type
    public Dictionary<GroundType, float> tileRatios;
}

public static class RuntimeLevelProgression
{
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

    /// <summary>
    /// Gets the Global difficulty modifier for the level
    /// </summary>
    /// <param name="difficultyDebt"></param>
    /// <returns></returns>
    static float GetGlobalDifficultyMultiplier(float difficultyDebt)
    {
        // Max global relief = 20%
        float maxRelief = 0.8f;

        // multiplier (1 -> 0.8, [0 -> 1]) => reduces the overall relief
        return Mathf.Lerp(1f, maxRelief, difficultyDebt);
    }


    // -------------------------
    // MAIN FUNCTION
    // -------------------------

    public static RuntimeLevelParameters GetParametersForLevel(
        int levelIndex,
        TileDatabase_SO tileDatabase,
        LevelCycleProgression_SO cycleProgression,
        int levelsPerCycle = 30, int livesLostThisLevel = 0, int failedTimes = 0,
        float globalDifficultyDebt = 0)
    {
        RuntimeLevelParameters p = new RuntimeLevelParameters();

        // -------------------------
        // BASE VALUES
        // -------------------------
        int minWidth = 3;
        int maxWidth = 7;
        int minHeight = 10;
        int maxHeight = 30;
        int minStarDistance = minHeight / 3;
        int maxStarDistance = maxHeight / 3;

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

        // 0 -> 20% of cycle
        // --- Increase Width ---
        if (cycleT <= phase0End)
        {
            float t = cycleT / phase0End;
            width = Mathf.RoundToInt(Mathf.Lerp(minWidth, maxWidth, t));
        }
        // 21 -> 40% of cycle
        // --- Max Width ---
        else if (cycleT <= phase1End)
        {
            width = maxWidth;
        }
        // 41 -> 60% of cycle
        // --- Increase Height + Max Width (+2) ---
        else if (cycleT <= phase2End)
        {
            float t = (cycleT - phase1End) / (phase2End - phase1End);
            width = Mathf.RoundToInt(Mathf.Lerp(maxWidth, maxWidth, t));
            height = Mathf.RoundToInt(Mathf.Lerp(minHeight, maxHeight, t));
        }
        // 61 -> 80% of cycle
        // --- Max Height + Max Width (+2) ---
        else if (cycleT <= phase3End)
        {
            width = maxWidth;
            height = maxHeight;
        }
        // After 80% (81% -> 100%)
        // --- Increase Star Distance ---
        else
        {
            width = maxWidth;
            height = maxHeight;
            float t = (cycleT - phase3End) / (1f - phase3End);
            starDistance = Mathf.RoundToInt(Mathf.Lerp(minStarDistance, maxStarDistance, t));
        }

        // -------------------------
        // ENVIRONMENTAL DIFFICULTY
        // -------------------------

        bool hasMaxRelief = HasMaxDifficultyRelief(
            livesLostThisLevel, failedTimes, globalDifficultyDebt);

        bool isRecovery = IsRecoveryLevel(
            cycleIndex, cycleLevel, hasMaxRelief);

        LevelArchetypeData_SO archetype = SelectArchetype(
            cycleProgression, cycleIndex, cycleLevel, isRecovery);

        // -------------------------
        // APPLY ARCHETYPE DATA
        // -------------------------

        ApplyArchetypeData(archetype, cycleT, 
            tileDatabase, out Dictionary<GroundType, float> tileRatios);


        // -------------------------
        // ADAPTIVE DIFFICULTY
        // -------------------------

        float localDifficultyModifier = GetDifficultyMultiplier(livesLostThisLevel, failedTimes);
        float globalDifficultyModifier = GetGlobalDifficultyMultiplier(globalDifficultyDebt);
        float finalMultiplier = localDifficultyModifier * globalDifficultyModifier;

        if (tileRatios != null)
        {
            foreach (var key in tileRatios.Keys.ToList())
            {
                tileRatios[key] *= finalMultiplier;
            }
        }

        // -------------------------
        // OUTPUT
        // -------------------------
        p.width = width;
        p.height = height;
        p.minStarDistance = Mathf.Clamp(starDistance, 1, 10);
        p.tileRatios = tileRatios;

        return p;
    }

    // -------------------------
    // RECOVERY LOGIC
    // -------------------------

    static bool IsRecoveryLevel(
    int cycleIndex, int cycleLevel, bool hasMaxRelief)
    {
        if (!hasMaxRelief)
            return false;

        int recoveryFrequency = Mathf.Clamp(6 - cycleIndex, 3, 6);
        return (cycleLevel % recoveryFrequency) == recoveryFrequency - 1;
    }

    // -------------------------
    // ARCHETYPE SELECTION
    // -------------------------

    static LevelArchetypeData_SO SelectArchetype(
        LevelCycleProgression_SO progression,
        int cycleIndex, int cycleLevel, bool isRecovery)
    {
        if (isRecovery)
            return progression.recoveryArchetype;

        if (progression == null || progression.cycles == null || progression.cycles.Count == 0)
        {
            Debug.LogError("LevelCycleProgression is NULL or empty");
            return null;
        }

        // Clamp cycle index to last defined cycle
        int safeCycleIndex = Mathf.Clamp(cycleIndex, 0, progression.cycles.Count - 1);
        var cycle = progression.cycles[safeCycleIndex];

        if (cycle.allowedArchetypes == null || cycle.allowedArchetypes.Count == 0)
        {
            Debug.LogError($"Cycle {safeCycleIndex} has no archetypes");
            return null;
        }

        // Deterministic selection
        int archetypeIndex = cycleLevel % cycle.allowedArchetypes.Count;
        return cycle.allowedArchetypes[archetypeIndex];
    }

    // -------------------------
    // ARCHETYPE APPLICATION
    // -------------------------

    static void ApplyArchetypeData(
    LevelArchetypeData_SO data, float t,
    TileDatabase_SO tileDatabase,
    out Dictionary<GroundType, float> tileRatios)
    {
        tileRatios = new Dictionary<GroundType, float>();

        if (data == null || data.modifiers == null)
            return;

        foreach (var mod in data.modifiers)
        {
            var tileDef = tileDatabase.GetByGround(mod.groundType);
            if (tileDef == null)
                continue;

            float scaled = Mathf.Clamp01(mod.weight * t);
            float maxRatio = tileDef.maxRatio;

            tileRatios[mod.groundType] =
                Mathf.Min(maxRatio, scaled * maxRatio);
        }
    }

    static bool HasMaxDifficultyRelief(
    int livesLostThisLevel, int failedTimes, float globalDifficultyDebt)
    {
        bool maxLocalRelief = livesLostThisLevel >= 3;
        bool maxGlobalRelief = globalDifficultyDebt >= 1f;

        return maxLocalRelief && maxGlobalRelief;
    }
}

using UnityEngine;

public static class LevelParameterGenerator
{
    // --- BASE VALUES (The absolute easiest settings) ---
    private const int BASE_SEED = 42; // A fixed starting seed for predictability
    private const int BASE_MAX_Z = 40;
    private const int BASE_PATH_DENSITY = 50; // Kept constant as per the request

    // --- PROGRESSION RULES ---

    // Path Width (Easy -> Hard)
    private static readonly int[] PATH_WIDTH_STEPS = { 6, 5, 4, 3, 2 }; // Starts at 6, ends at 2

    // Path Twistiness (Easy -> Hard)
    private const float TWISTINESS_MIN = 0f;
    private const float TWISTINESS_MAX = 100f;
    private const float TWISTINESS_INCREMENT = 25f; // 0, 25, 50, 75, 100

    // Map Size
    private const int MAX_Z_INCREMENT = 20;

    // --- CYCLE LENGTH CALCULATION ---
    // We pre-calculate how many levels are in each "cycle" to make the main function cleaner.

    // 1. How many steps are in a single Twistiness cycle? (e.g., 0, 25, 50, 75, 100 -> 5 steps)
    private const int TWISTINESS_CYCLE_LENGTH = (int)((TWISTINESS_MAX - TWISTINESS_MIN) / TWISTINESS_INCREMENT) + 1;

    // 2. How many levels are in a full PathWidth cycle?
    // This is complex because the spacing range changes. We need a helper function.
    private static readonly int LEVELS_PER_PATHWIDTH_CYCLE = CalculateLevelsPerWidthCycle();

    // 3. How many levels are in a full MaxZ cycle?
    // This is the total number of combinations before MaxZ increments.
    private static readonly int LEVELS_PER_MAX_Z_CYCLE = LEVELS_PER_PATHWIDTH_CYCLE * PATH_WIDTH_STEPS.Length;

    /// <summary>
    /// Calculates the parameters for a given level index based on an easy-to-hard progression.
    /// </summary>
    public static LevelParameters GenerateParametersForLevel(int levelIndex)
    {
        if (levelIndex < 0) levelIndex = 0;

        var p = new LevelParameters();
        int remainingIndex = levelIndex;

        // --- ALGORITHM ---

        // 1. Calculate MaxZ (Outermost loop)
        int maxZCycles = remainingIndex / LEVELS_PER_MAX_Z_CYCLE;
        p.MaxZ = BASE_MAX_Z + (maxZCycles * MAX_Z_INCREMENT);
        remainingIndex %= LEVELS_PER_MAX_Z_CYCLE;

        // 2. Calculate PathWidth (Second loop)
        int pathWidthCycles = remainingIndex / LEVELS_PER_PATHWIDTH_CYCLE;
        p.PathWidth = PATH_WIDTH_STEPS[pathWidthCycles];
        remainingIndex %= LEVELS_PER_PATHWIDTH_CYCLE;

        // 3. Calculate PathSpacing (Third loop)
        // This is the most complex part, as its cycle length depends on the current PathWidth.
        int spacingMin = GetMinSpacingForWidth(p.PathWidth);
        int spacingMax = 10;
        int spacingCycleLength = (spacingMax - spacingMin + 1) * TWISTINESS_CYCLE_LENGTH;

        int spacingCycles = remainingIndex / TWISTINESS_CYCLE_LENGTH;
        p.Spacing = spacingMin + spacingCycles;
        remainingIndex %= TWISTINESS_CYCLE_LENGTH;

        // 4. Calculate PathTwistiness (Innermost loop)
        p.PathTwistiness = TWISTINESS_MIN + (remainingIndex * TWISTINESS_INCREMENT);

        // 5. Set remaining and constant parameters
        p.Seed = BASE_SEED + levelIndex; // Seed increments uniquely for every level
        p.PathDensity = BASE_PATH_DENSITY; // Constant
        p.AllowBranching = false; // Constant
        p.GeneratedAutomatically = true;

        // Optional: Clamp values as a safeguard, though the logic should prevent overflows.
        p.PathTwistiness = Mathf.Clamp(p.PathTwistiness, TWISTINESS_MIN, TWISTINESS_MAX);
        p.Spacing = Mathf.Clamp(p.Spacing, spacingMin, spacingMax);

        return p;
    }

    /// <summary>
    /// Helper to get the dynamic minimum spacing based on path width.
    /// </summary>
    private static int GetMinSpacingForWidth(int pathWidth)
    {
        if (pathWidth >= 6) return 6;
        if (pathWidth <= 2) return 3;
        // Linear interpolation for widths between 6 and 2
        float t = Mathf.InverseLerp(6, 2, pathWidth); // 0 if width is 6, 1 if width is 2
        return (int)Mathf.Round(Mathf.Lerp(6, 3, t));
    }

    /// <summary>
    /// Helper to calculate the total number of combinations for a single PathWidth cycle.
    /// This is needed because the number of spacing steps is not constant.
    /// </summary>
    private static int CalculateLevelsPerWidthCycle()
    {
        int totalLevels = 0;
        int spacingMax = 10;
        // This assumes the progression always uses the same path width steps.
        // We are calculating the levels for ONE path width, but since the spacing range
        // changes, we take the largest possible range to create a uniform cycle length.
        // This simplifies the main logic significantly.
        int maxSpacingRange = spacingMax - GetMinSpacingForWidth(2) + 1; // Largest range (3 to 10)
        totalLevels = maxSpacingRange * TWISTINESS_CYCLE_LENGTH;
        return totalLevels;
    }
}

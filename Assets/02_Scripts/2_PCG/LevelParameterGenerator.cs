using UnityEngine;
public static class LevelParameterGenerator
{
    // --- BASE VALUES ---
    // These are the starting values for level 0.
    private const int BASE_SEED = 42; // A fixed starting seed for predictability
    private const int BASE_PATH_WIDTH = 3;
    private const int BASE_PATH_DENSITY = 50;
    private const int BASE_SPACING = 0;
    private const float BASE_PATH_TWISTINESS = 0f;

    // --- MODIFIERS ---
    // How much each stat changes per step.
    private const int SPACING_INCREMENT = 1;
    private const float TWISTINESS_INCREMENT = 1.0f;

    // --- CYCLE DEFINITION ---
    // This defines how many steps of one parameter occur before the next one increments.
    private const int TWISTINESS_CYCLE_LENGTH = 101; // (0 to 100)

    /// <summary>
    /// Generates a new set of level parameters based on the level index.
    /// </summary>
    /// <param name="levelIndex">The index of the level (e.g., 0, 1, 2...).</param>
    /// <returns>A new LevelGenerationParameters object with calculated values.</returns>
    public static LevelParameters GenerateParametersForLevel(int levelIndex)
    {
        if (levelIndex < 0)
        {
            levelIndex = 0; // Ensure index is not negative
        }

        var parameters = new LevelParameters();

        // --- ALGORITHM ---

        // 1. Calculate how many full "Spacing" cycles have been completed.
        int spacingCycles = levelIndex / TWISTINESS_CYCLE_LENGTH;

        // 2. Calculate the current step within the "Twistiness" cycle.
        // The modulo operator gives the remainder.
        int twistinessStep = levelIndex % TWISTINESS_CYCLE_LENGTH;

        // 3. Set the final parameter values based on the calculations.
        parameters.Seed = BASE_SEED + levelIndex; // Seed always increments by 1.
        parameters.PathWidth = BASE_PATH_WIDTH; // PathWidth is constant in this example.
        parameters.PathDensity = BASE_PATH_DENSITY;
        parameters.AllowBranching = false; // Also constant.

        // Spacing increases only after a full twistiness cycle.
        parameters.Spacing = BASE_SPACING + (spacingCycles * SPACING_INCREMENT);

        // Twistiness increases at each level and resets after a cycle.
        parameters.PathTwistiness = BASE_PATH_TWISTINESS + (twistinessStep * TWISTINESS_INCREMENT);

        // Optional: Clamp values to ensure they don't exceed limits.
        parameters.PathTwistiness = Mathf.Clamp(parameters.PathTwistiness, 0f, 100f);
        parameters.Spacing = Mathf.Clamp(parameters.Spacing, 0, 10); // Example max spacing

        parameters.GeneratedAutomatically = true;

        return parameters;
    }
}

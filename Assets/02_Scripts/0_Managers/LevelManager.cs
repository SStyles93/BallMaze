using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public Dictionary<int, LevelData> kvpLevelData = new Dictionary<int, LevelData>();
    [SerializeField] private PcgData_SO pcgData;
    [SerializeField] private GenerationParamameters_SO generationParamameters;

    #region Singleton
    public static LevelManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

    }
    #endregion

    public void InitializePCG(int index)
    {
        // Ensure the list is large enough. If not, generate and add parameters up to the required index.
        while (pcgData.levelParameters.Count <= index)
        {
            // Generate parameters for the next level in the sequence
            int nextLevelIndex = pcgData.levelParameters.Count;
            LevelParameters newParams = LevelParameterGenerator.GenerateParametersForLevel(nextLevelIndex);

            // Add the newly generated parameters to the list
            pcgData.levelParameters.Add(newParams);
        }

        // Now that we're sure the parameters exist at 'index', we can safely access them.
        LevelParameters targetParams = pcgData.levelParameters[index];

        generationParamameters.Seed = targetParams.Seed;
        generationParamameters.Spacing = targetParams.Spacing;
        generationParamameters.PathDensity = targetParams.PathDensity;
        generationParamameters.PathTwistiness = targetParams.PathTwistiness;
        generationParamameters.PathWidth = targetParams.PathWidth;
        generationParamameters.AllowBranching = targetParams.AllowBranching;
    }
}

using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public Dictionary<int, LevelData> KvpLevelData = new Dictionary<int, LevelData>();
    [SerializeField] private PcgData_SO pcgData;
    [SerializeField] private GenerationParamameters_SO generationParamameters;

    public int CurrentLevelIndex = 0;
    public LevelData CurrentLevelData = null;
    public float CurrentLevelTimeToComplete = 0;

    #region Singleton
    public static LevelManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

    }
    #endregion

    public void InitializeLevel(int index)
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
        
        CurrentLevelIndex = index;
        CurrentLevelData = new LevelData();
        CurrentLevelTimeToComplete = pcgData.levelParameters[index].timeToComplete;

        if (!KvpLevelData.ContainsKey(index))
        KvpLevelData.Add(index, CurrentLevelData);
    }

    public void SetLevelData(int lifeLeft, float levelTime)
    {
        if(CurrentLevelData == null)
        {
            Debug.Log("LevelData is null");
        }

        CurrentLevelData.levelScore = ((100 - (int)levelTime) - ((3-lifeLeft)*10));
        int levelGrade = 0;
        switch (CurrentLevelData.levelScore)
        {
            case > 80:
                levelGrade = 3;
                break;
            case >= 50:
                levelGrade = 2;
                break;
            case < 50:
                levelGrade = 1;
                break;
        }
        CurrentLevelData.levelGrade = levelGrade;
}
}

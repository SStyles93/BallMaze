using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public Dictionary<int, LevelData> KvpLevelData = new Dictionary<int, LevelData>();
    [SerializeField] private PcgData_SO pcgData;
    [SerializeField] private GenerationParamameters_SO generationParamameters;

    public LevelData CurrentLevelData = null;
    public int CurrentLevelIndex = 0;
    public int LifeLeftOnLevel = 0;
    public float TimeToCompleteLevel = 0;
    public int CurrencyToEarn = 0;
    public int CurrencyLeftOnLevel = 0;

    public float CurrentTimeToCompleteLevel = 0;

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
        CurrentLevelData = KvpLevelData[index];
        TimeToCompleteLevel = pcgData.levelParameters[index].timeToComplete;
        CurrencyToEarn = pcgData.levelParameters[index].currencyToEarn;

        if (!KvpLevelData.ContainsKey(index))
            KvpLevelData.Add(index, CurrentLevelData);
    }

    public void ProcessLevelData()
    {
        int grade = CalculateGradeFromData();
        int currencyEarned = CalculateCurrencyEarnedFromGrade(grade);

        if (currencyEarned <= 0)
            return;

        // If currency earned is bigger that what is left, return what is left
        if (currencyEarned >= CurrentLevelData.currencyLeftToEarn)
        {
            currencyEarned = CurrentLevelData.currencyLeftToEarn;
            CurrentLevelData.currencyLeftToEarn = 0;
        }
        // If what is earned is lower that what is left, return what is earned and remove that amount from what is left
        else
        {
            CurrentLevelData.currencyLeftToEarn -= currencyEarned;
        }

        CurrencyManager.Instance.currencyValue += currencyEarned;
    }

    public void RemoveCurrentLevelData()
    {
        KvpLevelData.Remove(CurrentLevelIndex);
    }

    public void SetLifeLeftOnLevel(int lifeLeft)
    {
        LifeLeftOnLevel = lifeLeft;
    }

    public void SetTimeValueOnLevel(float timeValue)
    {
        CurrentTimeToCompleteLevel = timeValue;
    }

    #region HELPER FUNCTIONS

    /// <summary>
    /// Calculates the currency using Grade
    /// </summary>
    /// <returns></returns>
    private int CalculateCurrencyEarnedFromGrade(int grade)
    {
        int currencyToReturn = CurrencyToEarn;
        currencyToReturn /= 3;
        currencyToReturn *= grade;
        currencyToReturn = Mathf.RoundToInt(currencyToReturn);

        return currencyToReturn;
    }

    /// <summary>
    /// Gets the grade of the level according to life and time
    /// </summary>
    /// <returns>a grade level [0-3]</returns>
    private int CalculateGradeFromData()
    {
        int gradeLevel = 3;
        
        gradeLevel -= (3-LifeLeftOnLevel);
        
        if(CurrentTimeToCompleteLevel <= TimeToCompleteLevel)
            gradeLevel -= 1;

        if(gradeLevel < 0)
        {
            Debug.LogError($"gradeLevel should not be lower than 0, current gradeLevel: {gradeLevel}");
        }

        return gradeLevel;
    }
    #endregion
}

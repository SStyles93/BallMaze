using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public Dictionary<int, LevelData> KvpLevelData = new Dictionary<int, LevelData>();
    [SerializeField] private PcgData_SO pcgData;
    [SerializeField] private GenerationParamameters_SO generationParamameters;

    private LevelData currentLevelData = null;
    private int currentLevelIndex = 0;
    private int lifeLeftOnLevel = 0;
    private float timeToCompleteLevel = 0;
    private int currencyToEarn = 0;
    private int previousGrade = 0;

    public float CurrentTimeToCompleteLevel = 0;

    #region Singleton
    public static LevelManager Instance { get; private set; }
    public int CurrentLevelIndex { get => currentLevelIndex; }
    public LevelData CurrentLevelData { get => currentLevelData; }

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

        // Get all the generation parameters from the PCGData_SO
        generationParamameters.Seed = targetParams.Seed;
        generationParamameters.Spacing = targetParams.Spacing;
        generationParamameters.PathDensity = targetParams.PathDensity;
        generationParamameters.PathTwistiness = targetParams.PathTwistiness;
        generationParamameters.PathWidth = targetParams.PathWidth;
        generationParamameters.AllowBranching = targetParams.AllowBranching;

        currentLevelIndex = index;

        // If no LevelData is present, create one with initial values
        if (!KvpLevelData.ContainsKey(index))
        {
            currentLevelData = new LevelData()
            {
                levelGrade = 0,
                levelScore = 0,
                currencyLeftToEarn = pcgData.levelParameters[index].currencyToEarn
            };
            KvpLevelData.Add(index, currentLevelData);
        }
        else
        {
            currentLevelData = KvpLevelData[index];
        }

        // Get the values of time and currency to earn from the SO
        timeToCompleteLevel = pcgData.levelParameters[index].timeToComplete;
        currencyToEarn = pcgData.levelParameters[index].currencyToEarn;

        previousGrade = KvpLevelData[index].levelGrade;
    }

    public void ProcessLevelData()
    {
        int grade = CalculateGradeFromData();

        currentLevelData.levelGrade = grade;

        int currencyEarned = CalculateCurrencyEarnedFromGrade(grade);

        if (currencyEarned <= 0)
            return;

        // If currency earned is bigger that what is left, return what is left
        if (currencyEarned >= currentLevelData.currencyLeftToEarn)
        {
            currencyEarned = currentLevelData.currencyLeftToEarn;
            currentLevelData.currencyLeftToEarn = 0;
        }
        // If what is earned is lower that what is left, return what is earned and remove that amount from what is left
        else
        {
            currentLevelData.currencyLeftToEarn -= currencyEarned;
        }

        CurrencyManager.Instance.IncreaseCurrency(currencyEarned);
    }

    public void RemoveCurrentLevelData()
    {
        KvpLevelData.Remove(currentLevelIndex);
    }

    public void SetLifeLeftOnLevel(int lifeLeft)
    {
        lifeLeftOnLevel = lifeLeft;
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
        int currencyToReturn = currencyToEarn;
        currencyToReturn /= 3;
        currencyToReturn *= (grade - previousGrade);
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

        gradeLevel -= (3 - lifeLeftOnLevel);

        if (CurrentTimeToCompleteLevel > timeToCompleteLevel)
            gradeLevel -= 1;

        if (gradeLevel < 0)
        {
            Debug.LogError($"gradeLevel should not be lower than 0, current gradeLevel: {gradeLevel}");
        }

        return gradeLevel;
    }
    #endregion
}

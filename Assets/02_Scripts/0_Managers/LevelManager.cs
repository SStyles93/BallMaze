using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public Dictionary<int, LevelData> LevelDataDictionnary = new Dictionary<int, LevelData>();
    [SerializeField] private PcgData_SO pcgData;
    [SerializeField] private GenerationParamameters_SO generationParamameters;

    private LevelData currentLevelData = null;
    private float currentTimeToCompleteLevel = 0;
    private float timeToCompleteLevel = 0;
    private int currentLevelIndex = 0;
    private int lifeLeftOnLevel = 0;
    private int currencyToEarn = 0;
    private int previousGrade = 0;
    private int previousScore = 0;


    #region Singleton
    public static LevelManager Instance { get; private set; }
    public int CurrentLevelIndex { get => currentLevelIndex; }
    public LevelData CurrentLevelData { get => currentLevelData; }
    public int PreviousGrade  => previousGrade;
    public int PreviousScore => previousScore;
    public float CurrentTimeToCompleteLevel => currentTimeToCompleteLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

    }
    #endregion

    /// <summary>
    /// Return the Grade for a Level saved in dictionnary
    /// </summary>
    /// <param name="levelIndex"></param>
    /// <returns></returns>
    public int GetGradeForLevelAtIndex(int levelIndex)
    {
        if(LevelDataDictionnary.ContainsKey(levelIndex))
        return LevelDataDictionnary[levelIndex].levelGrade;
        else return 0;
    }

    /// <summary>
    /// Initializes the Level at the given Index.
    /// </summary>
    /// <param name="index">Index of desired level to Initialize</param>
    /// <remarks>This method will init. LevelParameters, GenerationParameters, and LevelData</remarks>
    public void InitializeLevel(int index)
    {
        // Ensure the list is large enough. If not, generate and add parameters up to the required index.
        FillLevelParametersUpToIndex(index);

        // Now that we're sure the parameters exist at 'index', we can safely access them.
        SetGenerationParameters(index);

        currentLevelIndex = index;

        // If no LevelData is present, create one with initial values
        InitializeCurrentLevelData(index);

        // Get the values of time and currency to earn from the SO
        timeToCompleteLevel = pcgData.levelParameters[index].timeToComplete;
        currencyToEarn = pcgData.levelParameters[index].currencyToEarn;
        previousGrade = LevelDataDictionnary[index].levelGrade;
        previousScore = LevelDataDictionnary[index].levelScore;
    }

    /// <summary>
    /// Calculates the Grade and Currency value earned in the level
    /// </summary>
    public void ProcessLevelData()
    {

        int grade = CalculateGradeFromData(lifeLeftOnLevel, currentTimeToCompleteLevel);
        if(currentLevelData.levelGrade < grade)
        currentLevelData.levelGrade = grade;

        int score = CalulateScoreFromData(grade, currentTimeToCompleteLevel);
        if(currentLevelData.levelScore < score)
        currentLevelData.levelScore = score;

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

    /// <summary>
    /// Removes the current level data from the dictionnary of LevelDatas
    /// </summary>
    public void RemoveCurrentLevelData()
    {
        LevelDataDictionnary.Remove(currentLevelIndex);
    }

    /// <summary>
    /// Sets the lifeLeftOnLevel
    /// </summary>
    /// <param name="lifeLeft">value of life</param>
    public void SetLifeLeftOnLevel(int lifeLeft)
    {
        lifeLeftOnLevel = lifeLeft;
    }

    /// <summary>
    /// Sets the currentTimeToCompleteLevel
    /// </summary>
    /// <param name="timeValue">Time value (float)</param>
    public void SetTimeValueOnLevel(float timeValue)
    {
        currentTimeToCompleteLevel = timeValue;
    }

    #region PRIVATE FUNCTIONS

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
    /// <param name="lifeLeft">Life left on the player when the level ended</param>
    /// <param name="elapsedTime">Time to complete the level</param>
    /// <returns>a grade level [0-3]</returns>
    private int CalculateGradeFromData(int lifeLeft, float elapsedTime)
    {
        int gradeLevel = 3;

        gradeLevel -= (3 - lifeLeft);

        if (elapsedTime > timeToCompleteLevel)
            gradeLevel -= 1;

        if (gradeLevel < 0)
        {
            Debug.LogError($"gradeLevel should not be lower than 0, current gradeLevel: {gradeLevel}");
        }

        return gradeLevel;
    }

    /// <summary>
    /// Calculates the score for the game using the grade and the time
    /// </summary>
    /// <param name="grade">Grade [0-3]</param>
    /// <param name="time">Time value (float)</param>
    /// <returns>A score (int)</returns>
    private int CalulateScoreFromData(int grade, float time)
    {
        if (time <= 0) return 0;

        return Mathf.RoundToInt(1/time * (grade+1) * 1000);
    }

    /// <summary>
    /// Generates parameters for all levels up to the index value 
    /// </summary>
    /// <param name="index">target index</param>
    private void FillLevelParametersUpToIndex(int index)
    {
        while (pcgData.levelParameters.Count <= index)
        {
            // Generate parameters for the next level in the sequence
            int nextLevelIndex = pcgData.levelParameters.Count;
            LevelParameters newParams = LevelParameterGenerator.GenerateParametersForLevel(nextLevelIndex);

            // Add the newly generated parameters to the list
            pcgData.levelParameters.Add(newParams);
        }
    }

    /// <summary>
    /// Gets or Creates a LevelData container for the given index
    /// </summary>
    /// <param name="index">Index of the level</param>
    private void InitializeCurrentLevelData(int index)
    {
        if (!LevelDataDictionnary.ContainsKey(index))
        {
            currentLevelData = new LevelData()
            {
                levelGrade = 0,
                levelScore = 0,
                currencyLeftToEarn = pcgData.levelParameters[index].currencyToEarn
            };
            LevelDataDictionnary.Add(index, currentLevelData);
        }
        else
        {
            currentLevelData = LevelDataDictionnary[index];
        }
    }

    /// <summary>
    /// Sets the Generation Parameters for the given level index
    /// </summary>
    /// <param name="levelIndex">Index of the Level</param>
    private void SetGenerationParameters(int levelIndex)
    {
        LevelParameters targetParams = pcgData.levelParameters[levelIndex];

        generationParamameters.Seed = targetParams.Seed;
        generationParamameters.Spacing = targetParams.Spacing;
        generationParamameters.PathDensity = targetParams.PathDensity;
        generationParamameters.PathTwistiness = targetParams.PathTwistiness;
        generationParamameters.PathWidth = targetParams.PathWidth;
        generationParamameters.AllowBranching = targetParams.AllowBranching;
    }
    #endregion
}

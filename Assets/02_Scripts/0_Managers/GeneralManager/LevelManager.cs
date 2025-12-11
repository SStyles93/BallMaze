using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public Dictionary<int, LevelData> LevelDataDictionnary = new Dictionary<int, LevelData>();
    [SerializeField] private PcgData_SO pcgData;
    [SerializeField] private GenerationParamameters_SO generationParamameters;

    private LevelData currentLevelData = null;
    private int currentStarCount = 0;
    private int currentLevelIndex = 0;
    private int currencyToEarn = 0;
    private int previousNumberOfStarts = 0;
    private bool wasGamePreviouslyFinished = false;


    #region Singleton
    public static LevelManager Instance { get; private set; }
    public int CurrentLevelIndex { get => currentLevelIndex; }
    public LevelData CurrentLevelData { get => currentLevelData; }
    public int PreviousNumberOfStars  => previousNumberOfStarts;
    public bool WasGamePreviouslyFinished => wasGamePreviouslyFinished;
    public int CurrentStarCount => currentStarCount;

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
        return LevelDataDictionnary[levelIndex].numberOfStars;
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

        // If no LevelData is present, create one with initial values
        InitializeCurrentLevelData(index);

        currentLevelIndex = index;
        currentStarCount = 0;

        // Get the values of time and currency to earn from the SO
        currencyToEarn = pcgData.levelParameters[index].currencyToEarn;
        previousNumberOfStarts = LevelDataDictionnary[index].numberOfStars;
        wasGamePreviouslyFinished = LevelDataDictionnary[index].wasLevelFinished;
    }

    /// <summary>
    /// Calculates the Grade and Currency value earned in the level
    /// </summary>
    public void ProcessLevelData()
    {
        currentLevelData.wasLevelFinished = true;

        if (currentStarCount > previousNumberOfStarts)
            currentLevelData.numberOfStars = currentStarCount;

        int currencyEarned = CalculateCurrencyEarnedFromGrade(currentStarCount);
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
    /// Sets the currentTimeToCompleteLevel
    /// </summary>
    /// <param name="timeValue">Time value (float)</param>
    public void IncreaseStarCount()
    {
        currentStarCount++;
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
        currencyToReturn *= (grade - previousNumberOfStarts);
        currencyToReturn = Mathf.RoundToInt(currencyToReturn);

        return currencyToReturn;
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
                numberOfStars = 0,
                currencyLeftToEarn = pcgData.levelParameters[index].currencyToEarn,
                wasLevelFinished = false
            };
            LevelDataDictionnary.Add(index, currentLevelData);
        }
        else
        {
            currentLevelData = LevelDataDictionnary[index];
        }
        // Reset the number of stars at each level start
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

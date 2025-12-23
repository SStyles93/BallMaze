using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public Dictionary<int, LevelData> LevelDataDictionnary = new Dictionary<int, LevelData>();
    [SerializeField] GeneratorParameters_SO GeneratorParameters;
    [SerializeField] LevelDatabase_SO LevelDatabase;
    private TileType[,] currentGrid;


    private LevelData currentLevelData = null;
    private int currentStarCount = 0;
    private int currentLevelIndex = 0;
    private int currencyToEarn = 0;
    private int previousNumberOfStarts = 0;
    private bool wasGamePreviouslyFinished = false;

    public event Action<int> OnStarCountChanged;

    #region Singleton
    public static LevelManager Instance { get; private set; }
    public int CurrentLevelIndex { get => currentLevelIndex; }
    public LevelData CurrentLevelData { get => currentLevelData; }
    public int PreviousNumberOfStars => previousNumberOfStarts;
    public bool WasGamePreviouslyFinished => wasGamePreviouslyFinished;
    public int CurrentStarCount => currentStarCount;
    public TileType[,] CurrentGrid => currentGrid;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

    }
    #endregion
    public TileType[,] GenerateAndGetCurrentLevelGrid()
    {
        int usedSeed;

        TileType[,] grid = GenerateRuntimeLevel(
            currentLevelIndex,
            LevelDatabase,
            GeneratorParameters,
            out usedSeed
        );

        // Optional: store usedSeed somewhere if needed later
        return grid;
    }

    /// <summary>
    /// Return the Grade for a Level saved in dictionnary
    /// </summary>
    /// <param name="levelIndex"></param>
    /// <returns></returns>
    public int GetGradeForLevelAtIndex(int levelIndex)
    {
        if (LevelDataDictionnary.ContainsKey(levelIndex))
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
        // If no LevelData is present, create one with initial values
        InitializeCurrentLevelData(index);

        currentLevelIndex = index;
        currentStarCount = 0;

        // Get the values of time and currency to earn from the SO
//=====>>> TODO        //currencyToEarn = pcgData.levelParameters[index].currencyToEarn;
        previousNumberOfStarts = LevelDataDictionnary[index].numberOfStars;
        wasGamePreviouslyFinished = LevelDataDictionnary[index].wasLevelFinished;
        
        //Init level
        currentGrid = GenerateAndGetCurrentLevelGrid();
    }


    #region LEVEL DATA (GRADES)

    /// <summary>
    /// Calculates the Grade and Currency value earned in the level
    /// </summary>
    public void ProcessLevelData()
    {
        currentLevelData.wasLevelFinished = true;

        // --- STARS ---

        int earnedStars = currentStarCount - previousNumberOfStarts;
        // Make sure that the amount of stars is not negative
        // ex: previous try:3,
        //     current 2     ->     2-3 = -1
        if (earnedStars < 0) earnedStars = 0;

        CoinManager.Instance.IncreaseCurrencyAmount(CoinType.STAR, earnedStars);

        // Sets the StarCount for the level accounting for previously obtained stars
        if (currentStarCount > previousNumberOfStarts)
            currentLevelData.numberOfStars = currentStarCount;
        else
            currentStarCount = previousNumberOfStarts;


        // --- COINS ---

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

        CoinManager.Instance.IncreaseCurrencyAmount(CoinType.COIN, currencyEarned);
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
        OnStarCountChanged?.Invoke(currentStarCount);
    }

    #endregion

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
//=====>>> TODO                //currencyLeftToEarn = LevelDatabase.levels[index].currencyToEarn,
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
    /// Generates a level with RuntimeLevelProgression
    /// </summary>
    /// <param name="levelIndex">index of the level to generate/get</param>
    /// <param name="database">ref to the database </param>
    /// <param name="baseParameters">original parameters</param>
    /// <param name="usedSeed">OUT param. has to be declared but not necessarily used...</param>
    /// <returns>A 2D array of tiles (grid)</returns>
    private TileType[,] GenerateRuntimeLevel(int levelIndex,
        LevelDatabase_SO database, GeneratorParameters_SO baseParameters,
        out int usedSeed)
    {
        // If level already exists → load it
        LevelData_SO existing = database.GetLevelDataAtIndex(levelIndex);
        if (existing != null)
        {
            usedSeed = existing.usedSeed;
            return existing.ToGrid();
        }

        // Otherwise generate new parameters
        RuntimeLevelParameters runtimeParams =
            RuntimeLevelProgression.GetParametersForLevel(levelIndex);

        // Apply to generator parameters
        baseParameters.gridWidth = runtimeParams.width;
        baseParameters.gridHeight = runtimeParams.height;
        baseParameters.curvePercent = runtimeParams.curvePercent;
        baseParameters.minStarDistance = runtimeParams.minStarDistance;

        // Force random generation
        baseParameters.inputSeed = -1;

        return Generator.GenerateMaze(baseParameters, out usedSeed);
    }
    #endregion
}

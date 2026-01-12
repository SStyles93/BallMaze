using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public Dictionary<int, LevelData> levelDataDictionnary = new Dictionary<int, LevelData>();
    [SerializeField] GeneratorParameters_SO generatorParameters;
    [SerializeField] LevelDatabase_SO levelDatabase;
    [SerializeField] LevelArchetypeDatabase_SO levelArchetypeDatabase;
    [SerializeField] int levelsPerCycle = 30;
    [SerializeField] int initialCoinAmount = 30;
    private CellData[,] currentGrid;


    private LevelData currentLevelData = null;
    private int currentStarCount = 0;
    private int currentLevelIndex = 0;
    private int currencyToEarn = 0;
    private int previousNumberOfStarts = 0;
    private int livesLostToThisLevel = 0;
    private int failedTimes = 0;
    private bool wasGamePreviouslyFinished = false;

    public event Action<int> OnStarCountChanged;

    #region Singleton
    public static LevelManager Instance { get; private set; }
    public LevelDatabase_SO LevelDatabase { get => levelDatabase; set => levelDatabase = value; }
    public int CurrentLevelIndex { get => currentLevelIndex; }
    public LevelData CurrentLevelData { get => currentLevelData; }
    public int PreviousNumberOfStars => previousNumberOfStarts;
    public bool WasGamePreviouslyFinished => wasGamePreviouslyFinished;
    public int CurrentStarCount => currentStarCount;
    public CellData[,] CurrentGrid => currentGrid;


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

    }
    #endregion


    /// <summary>
    /// Generates the current level and returns its Cell grid
    /// </summary>
    public CellData[,] GenerateAndGetCurrentLevelGrid()
    {
        int usedSeed;

        // Use the new Cell[,] generator
        CellData[,] grid = this.GenerateRuntimeLevel(
            currentLevelIndex,
            levelDatabase,
            generatorParameters,
            out usedSeed
        );

        currentGrid = grid;

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
        if (levelDataDictionnary.ContainsKey(levelIndex))
            return levelDataDictionnary[levelIndex].numberOfStars;
        else return 0;
    }

    /// <summary>
    /// Initializes the Level at the given Index.
    /// </summary>
    /// <param name="index">Index of desired level to Initialize</param>
    /// <remarks>This method will init. LevelParameters, GenerationParameters, and LevelData</remarks>
    public void InitializeLevel(int index)
    {
        if (!CanStartLevel(index))
        {
            Debug.LogWarning($"Cannot start level {index} because previous level is unfinished.");
            return;
        }

        // If no LevelData is present, create one with initial values
        InitializeCurrentLevelData(index);

        currentLevelIndex = index;
        currentStarCount = 0;

        // Get the values of time and currency to earn from the SO
        currencyToEarn = levelDatabase.GetLevelDataAtIndex(index) == null ? initialCoinAmount : levelDatabase.GetLevelDataAtIndex(index).coinsToEarn;
        previousNumberOfStarts = levelDataDictionnary[index].numberOfStars;
        wasGamePreviouslyFinished = levelDataDictionnary[index].wasLevelFinished;
        livesLostToThisLevel = levelDataDictionnary[index].livesLostToThisLevel;
        failedTimes = levelDataDictionnary[index].failedTimes;

        //Init level
        currentGrid = GenerateAndGetCurrentLevelGrid();
    }

    public bool CanStartLevel(int index)
    {
        if (index == 0) return true; // First level is always allowed

        // Check if previous level was finished
        if (!levelDataDictionnary.ContainsKey(index - 1))
            return false;

        return levelDataDictionnary[index - 1].wasLevelFinished;
    }

    // Return the last index (count) of the LevelDataDictionnary
    public int GetHighestFinishedLevelIndex()
    {
        int highestFinished = -1;

        foreach (var kvp in levelDataDictionnary)
        {
            if (kvp.Value.wasLevelFinished && kvp.Key > highestFinished)
                highestFinished = kvp.Key;
        }

        return highestFinished;
    }

    #region LEVEL DATA (GRADES)

    /// <summary>
    /// Calculates the Grade and Currency value earned in the level
    /// </summary>
    public void ProcessLevelData()
    {
        currentLevelData.wasLevelFinished = true;
        currentLevelData.livesLostToThisLevel = livesLostToThisLevel;

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
        if (currencyEarned >= currentLevelData.coinsLeftToEarn)
        {
            currencyEarned = currentLevelData.coinsLeftToEarn;
            currentLevelData.coinsLeftToEarn = 0;
        }
        // If what is earned is lower that what is left, return what is earned and remove that amount from what is left
        else
        {
            currentLevelData.coinsLeftToEarn -= currencyEarned;
        }

        CoinManager.Instance.IncreaseCurrencyAmount(CoinType.COIN, currencyEarned);

        SavingManager.Instance?.SaveGame();
    }

    /// <summary>
    /// Removes the current level data from the dictionnary of LevelDatas
    /// </summary>
    public void MarkLevelAsFailed()
    {
        if (levelDataDictionnary.ContainsKey(currentLevelIndex))
        {
            currentLevelData.wasLevelFinished = false;
            currentLevelData.failedTimes++;
            currentLevelData.livesLostToThisLevel = livesLostToThisLevel;
            levelDataDictionnary[currentLevelIndex] = currentLevelData;
        }
        else
        {
            // First attempt at this level
            currentLevelData = new LevelData()
            {
                numberOfStars = 0,
                coinsLeftToEarn = initialCoinAmount,
                wasLevelFinished = false,
                livesLostToThisLevel = 0,
                failedTimes = 0
            };
            levelDataDictionnary.Add(currentLevelIndex, currentLevelData);
        }
    }

    public void IncreaseLivesLostToThisLevel()
    {
        livesLostToThisLevel++;
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
        if (!levelDataDictionnary.ContainsKey(index))
        {
            currentLevelData = new LevelData()
            {
                numberOfStars = 0,
                coinsLeftToEarn = initialCoinAmount,
                wasLevelFinished = false
            };
            levelDataDictionnary.Add(index, currentLevelData);
        }
        else
        {
            currentLevelData = levelDataDictionnary[index];
        }
        // Reset the number of stars at each level start
    }

    /// <summary>
    /// Generates a level with RuntimeLevelProgression
    /// </summary>
    /// <param name="levelIndex">index of the level to generate/get</param>
    /// <param name="database">reference to the level database</param>
    /// <param name="baseParameters">generator parameters</param>
    /// <param name="usedSeed">OUT param for the RNG seed</param>
    /// <returns>A 2D array of CellData (grid)</returns>
    private CellData[,] GenerateRuntimeLevel(
        int levelIndex,
        LevelDatabase_SO database,
        GeneratorParameters_SO baseParameters,
        out int usedSeed)
    {
        // 1️ If level already exists → load it
        LevelData_SO existing = database.GetLevelDataAtIndex(levelIndex);
        if (existing != null)
        {
            usedSeed = existing.usedSeed;
            return existing.ToGrid();
        }

        // 2️ Otherwise, generate runtime parameters
        RuntimeLevelParameters runtimeParams =
            RuntimeLevelProgression.GetParametersForLevel(levelIndex, 
            levelArchetypeDatabase, levelsPerCycle, 
            livesLostToThisLevel, failedTimes);

        // 3️ Apply runtime parameters to generator
        baseParameters.gridWidth = runtimeParams.width;
        baseParameters.gridHeight = runtimeParams.height;

        // 4 Path Settings
        baseParameters.emptyRatio = runtimeParams.emptyRatio;
        baseParameters.iceRatio = runtimeParams.iceRatio;
        baseParameters.movingPlatformRatio = runtimeParams.movingPlatformRatio;

        // Star Settings
        baseParameters.minStarDistance = runtimeParams.minStarDistance;
        baseParameters.coinsToEarn = existing == null ? 30 : existing.coinsToEarn;

        // 4️ Force random seed
        baseParameters.inputSeed = -1;

        // 5️ Generate new grid
        CellData[,] grid = PxP.PCG.Generator.GenerateMaze(baseParameters, out usedSeed);

        return grid;
    }
    #endregion
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class LevelManager : MonoBehaviour
{
    public Dictionary<int, LevelData> levelDataDictionnary = new Dictionary<int, LevelData>();

    [Header("PCG Parameters")]
    [SerializeField] GeneratorParameters_SO generatorParameters;
    [SerializeField] LevelDatabase_SO levelDatabase;
    [SerializeField] TileDatabase_SO tileDatabase_SO;

    [Header("Progression")]
    [SerializeField] GlobalDifficultyState_SO globalDifficultyModifier;
    [SerializeField] LevelCycleProgression_SO levelCycleProgression_SO;
    [SerializeField] int levelsPerCycle = 30;

    [Header("Coin & Currencies")]
    [SerializeField] int initialCoinAmount = 60;


    private Grid currentGrid;
    private static readonly Vector2Int[] CardinalDirections =
    { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right};

    private LevelData currentLevelData = null;
    private int currentLevelIndex = 0;
    private int currentStarCount = 0;
    private int currencyEarnedThisLevel = 0;
    private int currentLivesLostToThisLevel = 0;
    private int previousLivesLostToThisLevel = 0;
    private int previousStarCount = 0;
    private int failedTimes = 0;
    private bool wasGamePreviouslyFinished = false;

    public event Action<int> OnStarCountChanged;

    #region Singleton & Getters
    public static LevelManager Instance { get; private set; }
    public GlobalDifficultyState_SO GlobalDifficultyModifier => globalDifficultyModifier;
    public LevelData CurrentLevelData => currentLevelData;
    public int CurrentLevelIndex => currentLevelIndex;
    public int CurrencyEarnedThisLevel => currencyEarnedThisLevel;
    public int PreviousNumberOfStars => previousStarCount;
    public bool WasGamePreviouslyFinished => wasGamePreviouslyFinished;
    public int CurrentStarCount { get => currentStarCount; set => currentStarCount = value; }
    public Grid CurrentGrid => currentGrid;

    public event Action<int> OnLifeLostToThisLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

    }
    #endregion

    // --- GRID GENEREATION ---

    /// <summary>
    /// Generates the current level and returns its Cell grid
    /// </summary>
    public Grid GenerateAndGetCurrentLevelGrid()
    {
        int usedSeed;

        // Use the Cell[,] generator
        Grid grid = this.GenerateRuntimeLevel(
            currentLevelIndex,
            levelDatabase,
            generatorParameters,
            out usedSeed
        );

        currentGrid = grid;
        return grid;
    }

    #region LEVEL CHECK SYSTEM

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

        // Set the Current Level values
        currentLevelIndex = index;
        currentStarCount = 0;
        currentLivesLostToThisLevel = 0;

        previousStarCount = levelDataDictionnary[index].numberOfStars;
        wasGamePreviouslyFinished = levelDataDictionnary[index].wasLevelFinished;
        previousLivesLostToThisLevel = levelDataDictionnary[index].livesLostToThisLevel;
        failedTimes = levelDataDictionnary[index].failedTimes;

        //Init level
        currentGrid = GenerateAndGetCurrentLevelGrid();
    }

    public void LoadLevel(int indexOfLevelToPlay, SceneController.SceneTransitionPlan customPlan = null)
    {
        if (Enum.TryParse<SceneDatabase.Scenes>(
                SceneManager.GetActiveScene().name, out SceneDatabase.Scenes scene))
        {
            SceneController.SceneTransitionPlan plan = customPlan == null ? 
                SceneController.Instance.NewTransition() : customPlan;

            #region Tutorial

            // Movement Tutorial
            if (indexOfLevelToPlay == 1 && !TutorialManager.Instance.IsTutorial1Complete)
            {
                plan.Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.Tutorial1);
            }

            // Rocket Tutorial
            else if (indexOfLevelToPlay == 10 && !TutorialManager.Instance.IsTutorialRocketComplete)
            {
                plan.Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.TutorialRocket);
            }

            // Rocket Tutorial
            else if (indexOfLevelToPlay == 20 && !TutorialManager.Instance.IsTutorialUfoComplete)
            {
                plan.Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.TutorialUfo);
            }

            #endregion
            else
            {
                plan.Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.Game);
            }

            plan.Unload(scene)
            .WithOverlay()
            .Perform();
        }
    }

    public bool CanStartLevel(int index)
    {
        if (index == 1) return true; // First level is always allowed (0 is not a level (empty))

        // Check if previous level was finished
        if (!levelDataDictionnary.ContainsKey(index - 1))
            return false;

        return levelDataDictionnary[index - 1].wasLevelFinished;
    }

    // Return the last index (count) of the LevelDataDictionnary
    public int GetHighestFinishedLevelIndex()
    {
        int highestFinished = 0;

        foreach (var kvp in levelDataDictionnary)
        {
            if (kvp.Value.wasLevelFinished && kvp.Key > highestFinished)
                highestFinished = kvp.Key;
        }

        return highestFinished;
    }

    #endregion

    #region GLOBAL DIFFICULTY

    void ConsumeGlobalDifficulty()
    {
        if (globalDifficultyModifier.remainingLevels <= 0)
        {
            globalDifficultyModifier.difficultyDebt = 0f;
            return;
        }

        globalDifficultyModifier.remainingLevels--;
    }

    void UpdateGlobalDifficulty(int livesLost)
    {
        if (livesLost <= 0) return;
        // Losing lives reduces the difficulty
        float addedDebt = livesLost * 0.1f;

        globalDifficultyModifier.difficultyDebt =
            Mathf.Clamp01(globalDifficultyModifier.difficultyDebt + addedDebt);

        globalDifficultyModifier.remainingLevels = 4; // lasts next 4 levels
    }

    #endregion

    #region LEVEL DATA (GRADES)

    /// <summary>
    /// Calculates the Grade and Currency value earned in the level
    /// </summary>
    public void ProcessLevelData()
    {
        // --- STARS ---

        int earnedStars = currentStarCount - previousStarCount;
        // Make sure that the amount of stars is not negative
        // ex: previous try:3,
        //     current 2     ->     2-3 = -1
        if (earnedStars < 0) earnedStars = 0;

        // Sets the StarCount for the level accounting for previously obtained stars
        if (currentStarCount > previousStarCount)
            currentLevelData.numberOfStars = currentStarCount;
        else
            currentStarCount = previousStarCount;


        // --- COINS ---

        int currentScore = CalculateGradeScore(currentStarCount, currentLivesLostToThisLevel);
        int previousScore;

        // If the level was never finished, treat previous as "nothing earned yet"
        if (currentLevelData.wasLevelFinished)
            previousScore = CalculateGradeScore(previousStarCount, previousLivesLostToThisLevel);
        else
            previousScore = 0; // first completion always counts


        int currentCoins = GradeToCoins(currentScore);
        int previousCoins = GradeToCoins(previousScore);

        int currencyEarned = Mathf.Max(0, currentCoins - previousCoins);

        // Ensure there is no gain over the max amount
        if (currencyEarned >= currentLevelData.coinsLeftToEarn)
            currencyEarned = currentLevelData.coinsLeftToEarn;


        currentLevelData.coinsLeftToEarn -= currencyEarned;

        // Used by the UICurrencyAnimator on the End Pannel
        currencyEarnedThisLevel = currencyEarned;


        // --- GLOBAL DIFFICULTY MODIFIER ---

        currentLevelData.livesLostToThisLevel = currentLivesLostToThisLevel;
        ConsumeGlobalDifficulty();
        UpdateGlobalDifficulty(currentLivesLostToThisLevel);


        // --- MANAGERS UPDATE - STARS, COINS ---

        currentLevelData.wasLevelFinished = true;
        CoinManager.Instance.IncreaseCurrencyAmount(CoinType.STAR, earnedStars);
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
            currentLevelData.failedTimes++;
            currentLevelData.livesLostToThisLevel = previousLivesLostToThisLevel;
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
                livesLostToThisLevel = 3,
                failedTimes = 1
            };
            levelDataDictionnary.Add(currentLevelIndex, currentLevelData);
        }

        UpdateGlobalDifficulty(currentLivesLostToThisLevel);
    }

    public void IncreaseLivesLostToThisLevel()
    {
        currentLivesLostToThisLevel++;

        //Used by the PlayerMovement to increase the ground radius detection (help player)
        OnLifeLostToThisLevel?.Invoke(currentLivesLostToThisLevel);
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
    /// Calculates the Grade
    /// </summary>
    /// <returns></returns>
    private int CalculateGradeScore(int starCount, int livesLost)
    {
        // Star contribution
        int starScore = 0;
        switch (starCount)
        {
            case 1: starScore = 2; break;
            case 2: starScore = 4; break;
            case 3: starScore = 6; break;
            default: starScore = 0; break; // 0 stars
        }

        // Lives contribution
        int livesScore = 0;
        switch (livesLost)
        {
            case 0: livesScore = 3; break;
            case 1: livesScore = 2; break;
            case 2: livesScore = 1; break;
            default: livesScore = 0; break; // 3+ lives lost -> fail
        }

        return starScore + livesScore;
    }

    private int GradeToCoins(int score)
    {
        return score switch
        {
            9 => 60,
            8 => 50,
            7 => 45,
            6 => 35,
            5 => 25,
            4 => 20,
            3 => 15,
            2 => 10,
            1 => 5,
            _ => 0
        };
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
    private Grid GenerateRuntimeLevel(
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
            RuntimeLevelProgression.GetParametersForLevel(levelIndex, tileDatabase_SO,
            levelCycleProgression_SO, levelsPerCycle,
            previousLivesLostToThisLevel, failedTimes,
            globalDifficultyModifier.difficultyDebt);

        // 3️ Apply runtime parameters to generator
        baseParameters.gridWidth = runtimeParams.width;
        baseParameters.gridHeight = runtimeParams.height;

        // 4 Path Settings
        baseParameters.emptyRatio = runtimeParams.emptyRatio;
        baseParameters.iceRatio = runtimeParams.iceRatio;
        baseParameters.movingPlatformRatio = runtimeParams.movingPlatformRatio;
        baseParameters.piquesRatio = runtimeParams.piqueRatio;
        baseParameters.doorDownRatio = runtimeParams.doorDownRatio;
        baseParameters.doorUpRatio = runtimeParams.doorUpRatio;

        // Star Settings
        baseParameters.minStarDistance = runtimeParams.minStarDistance;
        baseParameters.coinsToEarn = existing == null ? 30 : existing.coinsToEarn;

        // 4️ Force random seed
        baseParameters.inputSeed = -1;

        // 5️ Generate new grid
        Grid grid = PxP.PCG.Generator.GenerateMaze(baseParameters, out usedSeed);

        return grid;
    }

    #endregion
}

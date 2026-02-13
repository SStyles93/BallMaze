using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SavingManager : MonoBehaviour
{
    public static SavingManager Instance { get; private set; }

    private IDataService dataService;

    public GameData currentGameData = null;
    public PlayerData currentPlayerData = null;
    public SkinShopData currentSkinShopData = null;
    public SettingsData currentSettingsData = null;
    public TutorialData currentTutorialData = null;

    const string GameDataFileName = "GameData";
    const string PlayerDataFileName = "PlayerData";
    const string SkinDataFileName = "ShopData";
    const string SettingsDataFileName = "SettingsData";
    const string TutorialDataFileName = "TutorialsData";

    private void OnEnable()
    {
        CloudSaveManager.Instance.OnCloudLoadCompleted += LoadSession;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

        dataService = new JsonDataService(); // Or any other IDataService implementation
    }

    // --- CLOUD RELATED METHODS ---

    public T Get<T>() where T : SaveableData
    {
        if (typeof(T) == typeof(GameData))
            return currentGameData as T;

        if (typeof(T) == typeof(PlayerData))
            return currentPlayerData as T;

        if (typeof(T) == typeof(SkinShopData))
            return currentSkinShopData as T;

        if (typeof(T) == typeof(TutorialData))
            return currentTutorialData as T;

        return null;
    }

    public void ForceLocalOverwrite<T>(T data, bool saveToDisk = true) where T : SaveableData
    {
        if (data == null) return;

        switch (data)
        {
            case GameData g:
                currentGameData = g;
                if (saveToDisk) SaveDataInFile(g, GameDataFileName);
                break;

            case PlayerData p:
                currentPlayerData = p;
                if (saveToDisk) SaveDataInFile(p, PlayerDataFileName);
                break;

            case SkinShopData s:
                currentSkinShopData = s;
                if (saveToDisk) SaveDataInFile(s, SkinDataFileName);
                break;

            case TutorialData t:
                currentTutorialData = t;
                if (saveToDisk) SaveDataInFile(t, TutorialDataFileName);
                break;
        }
    }

    /// <summary>
    /// Deletes all the local files<br/> 
    /// Creates new empty data classes<br/>
    /// Restores all the managers to the new data
    /// </summary>
    public void DeleteAllData()
    {
        dataService.Delete(PlayerDataFileName);
        dataService.Delete(GameDataFileName);
        dataService.Delete(SkinDataFileName);
        dataService.Delete(SettingsDataFileName);
        dataService.Delete(TutorialDataFileName);

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif

        RestoreGameDataFromFile(GameDataFileName);
        RestoreSkinShopDataFromFile(SkinDataFileName);
        RestorePlayerDataFromFile(PlayerDataFileName);
        RestoreSettingsDataFromFile(SettingsDataFileName);
        RestoreTutorialDataFromFile(TutorialDataFileName);
    }

    // --- SAVE ---

    /// <summary>
    /// Saves the session in a Session.Json file
    /// </summary>
    public void SaveSession()
    {
        SaveGameDataInFile(GameDataFileName);

        SavePlayerDataInFile(PlayerDataFileName);

        SaveCustomizationShopDataInFile(SkinDataFileName);

        SaveSettingsDataInFile(SettingsDataFileName);

        SaveTutorialDataInFile(TutorialDataFileName);

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }


    public void SaveGame()
    {
        SaveGameDataInFile(GameDataFileName);
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
    public void SavePlayer()
    {
        SavePlayerDataInFile(PlayerDataFileName);
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
    public void SaveShop()
    {
        SaveCustomizationShopDataInFile(SkinDataFileName);
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
    public void SaveSettings()
    {
        SaveSettingsDataInFile(SettingsDataFileName);
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
    public void SaveTutorials()
    {
        SaveTutorialDataInFile(SettingsDataFileName);
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }


    /// <summary>
    /// Saves the TimeStamp & Levels in a "GameData.json" files
    /// </summary>
    private void SaveGameDataInFile(string fileName)
    {
        if (currentGameData == null)
        {
            currentGameData = new GameData();
        }

        SaveLevelsDataInGameData(currentGameData);

        SaveDataInFile(currentGameData, fileName);
    }

    /// <summary>
    /// Captures the data of all levels (lvl n° (int), grade (int), score (int))
    /// </summary>
    private void SaveLevelsDataInGameData(GameData gameData)
    {
        if (LevelManager.Instance == null)
        {
            //Debug.Log("No LevelManager instance available");
            return;
        }

        foreach (var kvp in LevelManager.Instance.levelDataDictionnary)
        {
            if (gameData.levelsData.ContainsKey(kvp.Key))
                gameData.levelsData[kvp.Key] = kvp.Value;
            else
                gameData.levelsData.Add(kvp.Key, kvp.Value);

        }
        gameData.difficultyDebt = LevelManager.Instance.GlobalDifficultyModifier.difficultyDebt;
        gameData.remainingLevels = LevelManager.Instance.GlobalDifficultyModifier.remainingLevels;
    }

    /// <summary>
    /// Captures the player's data (currency (int), colorIndex(int), materialIndex(int))
    /// </summary>
    private void SavePlayerDataInFile(string fileName)
    {
        PlayerData playerSaveData = new PlayerData();

        // --- COIN ---
        CoinManager coinManager = CoinManager.Instance;
        if (coinManager == null)
        {
            //Debug.Log("No CurrencyManager instance available");
            //in case of fail we still want to try and access the shopmanager
            goto ShopManager;
        }
        // Coins
        playerSaveData.coins = coinManager.GetCoinAmount(CoinType.COIN);
        playerSaveData.stars = coinManager.GetCoinAmount(CoinType.STAR);
        playerSaveData.hearts = coinManager.GetCoinAmount(CoinType.HEART);
        playerSaveData.rockets = coinManager.GetCoinAmount(CoinType.ROCKET);
        playerSaveData.ufos = coinManager.GetCoinAmount(CoinType.UFO);

        // Timers
        playerSaveData.lastHeartRefillTime = coinManager.LastHeartRefillTime;
        playerSaveData.lastCoinVideoTime = coinManager.LastVideoRewardTime;


        // --- SHOP ---
        ShopManager:
        CustomizationManager shopManager = CustomizationManager.Instance;
        if (shopManager == null)
        {
            //Debug.Log("No ShopManager instance available");
            return;
        }
        playerSaveData.colorIndex = shopManager.skinData_SO.playerColorIndex;
        playerSaveData.skinIndex = shopManager.skinData_SO.playerSkinIndex;


        // --- SAVING ---
        currentPlayerData = playerSaveData;
        SaveDataInFile(currentPlayerData, fileName);
    }

    /// <summary>
    /// Method to capture the state of settings
    /// </summary>
    /// <param name="fileName"></param>
    private void SaveSettingsDataInFile(string fileName)
    {
        SettingsData settingsData = new SettingsData()
        {
            isMusicOn = true,
            isAudioOn = true,
            isVibrationOn = true,
        };

        if (AudioManager.Instance != null)
        {
            // Gets the state of Music, Environment & SFX
            settingsData.isAudioOn = AudioManager.Instance.IsAudioEnabled;
            // State of Music Only
            settingsData.isMusicOn = AudioManager.Instance.IsMusicEnabled;
        }
        if (VibrationManager.Instance != null)
        {
            settingsData.isVibrationOn = VibrationManager.Instance.IsVibrationActive;
        }

        currentSettingsData = settingsData;

        SaveDataInFile(currentSettingsData, fileName);
    }

    /// <summary>
    /// Method to capture the state of customization options
    /// </summary>
    private void SaveCustomizationShopDataInFile(string fileName)
    {
        if (CustomizationManager.Instance == null) return;

        CustomizationData_SO currentDataSO = CustomizationManager.Instance.customizationData_SO;

        SkinShopData shopData = new SkinShopData
        {
            colorsLockedState = new Dictionary<string, bool>(),
            skinsLockedState = new Dictionary<string, bool>()
        };


        // Save materials state (un-locked)
        foreach (var skinOption in currentDataSO.skins)
        {
            shopData.skinsLockedState[skinOption.Id] = skinOption.isLocked;
        }

        // Save colors state (un-locked)
        foreach (var colorOption in currentDataSO.colors)
        {
            shopData.colorsLockedState[colorOption.Id] = colorOption.isLocked;
        }

        currentSkinShopData = shopData;

        SaveDataInFile(currentSkinShopData, fileName);
    }

    /// <summary>
    /// Saves the Tutorial Data in File <br/>
    /// (Tutorial1, Shop, Rocket, Ufo)
    /// </summary>
    /// <param name="fileName"></param>
    private void SaveTutorialDataInFile(string fileName)
    {
        TutorialData tutorialData = new TutorialData
        {
            isTutorial1Complete = false,
            isTutorialShopComplete = false,
            isTutorialRocketComplete = false,
            isTutorialUfoComplete = false,
            wasCoinsReceived = false,
            wasRocketReceived = false,
            wasUfoReceived = false,
        };

        if (TutorialManager.Instance == null) return;

        tutorialData.isTutorial1Complete = TutorialManager.Instance.IsTutorial1Complete;
        tutorialData.isTutorialShopComplete = TutorialManager.Instance.IsTutorialShopComplete;
        tutorialData.isTutorialRocketComplete = TutorialManager.Instance.IsTutorialRocketComplete;
        tutorialData.isTutorialUfoComplete = TutorialManager.Instance.IsTutorialUfoComplete;
        // Gifts
        tutorialData.wasCoinsReceived = CoinManager.Instance.WasCoinsReceived;
        tutorialData.wasRocketReceived = CoinManager.Instance.WasRocketReceived;
        tutorialData.wasUfoReceived = CoinManager.Instance.WasUfoReceived;

        currentTutorialData = tutorialData;
        SaveDataInFile(currentTutorialData, fileName);
    }



    // --- LOAD ---

    /// <summary>
    /// Loads the session from a Session.Json file
    /// </summary>
    public void LoadSession()
    {
        RestoreGameDataFromFile(GameDataFileName);

        RestorePlayerDataFromFile(PlayerDataFileName);

        RestoreSkinShopDataFromFile(SkinDataFileName);

        RestoreSettingsDataFromFile(SettingsDataFileName);

        RestoreTutorialDataFromFile(TutorialDataFileName);
    }


    public void LoadGame()
    {
        RestoreGameDataFromFile(GameDataFileName);
    }
    public void LoadPlayer()
    {
        RestorePlayerDataFromFile(PlayerDataFileName);
    }
    public void LoadShop()
    {
        RestoreSkinShopDataFromFile(SkinDataFileName);
    }
    public void LoadSettings()
    {
        RestoreSettingsDataFromFile(SettingsDataFileName);
    }
    public void LoadTutorials()
    {
        RestoreTutorialDataFromFile(TutorialDataFileName);
    }


    /// <summary>
    /// Loads the Game Data and Levels Data
    /// </summary>
    /// <param name="fileName"></param>
    private void RestoreGameDataFromFile(string fileName)
    {
        currentGameData = LoadFile<GameData>(fileName);
        if (currentGameData == null)
        {
            //Debug.Log($"file {fileName} does not exist, creating new GameData");
            currentGameData = new GameData()
            {
                firstTimestamp = DateTime.UtcNow
            };
        }

        RestoreLevelsDataFromGameData(currentGameData);
    }

    /// <summary>
    /// Restores the data for all levels (lvl n°, grade(int), score(int)
    /// </summary>
    private void RestoreLevelsDataFromGameData(GameData gameData)
    {
        if (LevelManager.Instance == null) return;

        LevelManager.Instance.levelDataDictionnary.Clear();

        if (gameData.levelsData == null)
        {
            LevelData levelsData = new LevelData();

            //Debug.Log("Current Session Data does not exist, creating new levelsData");
        }
        // --- DEBUG: Creates "finished" leveldata for all levels
        if (CoreManager.Instance.unlockAllLevels)
        {
            LevelData levelData = new LevelData()
            {
                numberOfStars = 3,
                coinsLeftToEarn = 0,
                livesLostToThisLevel = 0,
                failedTimes = 0,
                wasLevelFinished = true
            };
            for (int i = 1; i <= CoreManager.Instance.numberOfLevels; i++)
            {
                LevelManager.Instance.levelDataDictionnary.Add(i, levelData);
            }
        }
        else
        {
            // Retrieves the LevelDatas and sets them in the LevelManager
            foreach (var kvp in gameData.levelsData)
            {
                LevelManager.Instance.levelDataDictionnary[kvp.Key] = kvp.Value;
            }
        }

        LevelManager.Instance.GlobalDifficultyModifier.difficultyDebt = gameData.difficultyDebt;
        LevelManager.Instance.GlobalDifficultyModifier.remainingLevels = gameData.remainingLevels;
    }

    /// <summary>
    /// Restores the data for the player (currency (int))
    /// </summary>
    private void RestorePlayerDataFromFile(string fileName)
    {
        currentPlayerData = LoadFile<PlayerData>(fileName);

        if (currentPlayerData == null)
        {
            PlayerData playerData = new PlayerData()
            {
                lastHeartRefillTime = DateTime.UtcNow,
                lastCoinVideoTime = DateTime.UtcNow.AddDays(-1),
                coins = 0,
                stars = 0,
                hearts = CoinManager.Instance.InitialHeartAmount,
                rockets = 0,
                ufos = 0,
                colorIndex = 0,
                skinIndex = 0,
            };
            //Debug.Log("Current Session Data does not exist, creating new PlayerData");
            currentPlayerData = playerData;
        }

        // --- COINS ---
        CoinManager coinManager = CoinManager.Instance;
        if (coinManager == null)
        {
            //Debug.Log("No CurrencyManager instance available");
            goto ShopManager;
        }
        coinManager.SetCurrencyAmount(CoinType.COIN, currentPlayerData.coins);
        coinManager.LevelPreviousCoinAmount(CoinType.COIN);

        coinManager.SetCurrencyAmount(CoinType.STAR, currentPlayerData.stars);
        coinManager.LevelPreviousCoinAmount(CoinType.STAR);

        coinManager.SetCurrencyAmount(CoinType.HEART, currentPlayerData.hearts);
        coinManager.LevelPreviousCoinAmount(CoinType.HEART);

        coinManager.SetCurrencyAmount(CoinType.ROCKET, currentPlayerData.rockets);
        coinManager.LevelPreviousCoinAmount(CoinType.ROCKET);

        coinManager.SetCurrencyAmount(CoinType.UFO, currentPlayerData.ufos);
        coinManager.LevelPreviousCoinAmount(CoinType.UFO);

        coinManager.SetLastHeartRefillTime(currentPlayerData.lastHeartRefillTime);
        coinManager.SetLastCoinVideoTime(currentPlayerData.lastCoinVideoTime);
        LifeManager.Instance.ResetLife();

        // --- SHOP ---
        ShopManager:
        CustomizationManager shopManager = CustomizationManager.Instance;
        if (shopManager == null)
        {
            //Debug.Log("No ShopManager instance available");
            return;
        }
        shopManager.skinData_SO.skinOption = shopManager.customizationData_SO.skins[currentPlayerData.skinIndex];
        shopManager.skinData_SO.playerSkinIndex = currentPlayerData.skinIndex;
        shopManager.skinData_SO.colorOption = shopManager.customizationData_SO.colors[currentPlayerData.colorIndex];
        shopManager.skinData_SO.playerColorIndex = currentPlayerData.colorIndex;
    }

    /// <summary>
    /// Restore the values of settings from the SettingsData
    /// </summary>
    /// <param name="fileName"></param>
    private void RestoreSettingsDataFromFile(string fileName)
    {
        currentSettingsData = LoadFile<SettingsData>(fileName);

        if (currentSettingsData == null)
        {
            //Debug.Log("Current SettingsData does not exist, creating a new one");
            currentSettingsData = new SettingsData();
        }

        AudioManager audioManager = AudioManager.Instance;
        if (audioManager != null)
        {
            audioManager.SetGeneralAudioState(currentSettingsData.isAudioOn);
            audioManager.SetMusicState(currentSettingsData.isMusicOn);
        }

        VibrationManager vibrationManager = VibrationManager.Instance;
        if (vibrationManager != null)
        {
            vibrationManager.SetVibrationManagerState(currentSettingsData.isVibrationOn);
        }
    }

    /// <summary>
    /// Restores all the customization states from the ShopData (color.isLocked & material.isLocked)
    /// </summary>
    private void RestoreSkinShopDataFromFile(string fileName)
    {
        if (CustomizationManager.Instance == null) return;

        currentSkinShopData = LoadFile<SkinShopData>(fileName);

        var dataSO = CustomizationManager.Instance.customizationData_SO;

        if (currentSkinShopData == null)
        {
            currentSkinShopData = new SkinShopData();
            dataSO.ResetData();
            return;
        }

        // Restore skins state (un-locked)
        foreach (var skinOption in dataSO.skins)
        {
            if (currentSkinShopData.skinsLockedState.TryGetValue(skinOption.Id, out bool locked))
            {
                skinOption.isLocked = locked;
            }
            else // new Skin => Locked by default
            {
                skinOption.isLocked = true;
            }
        }

        // Restore colors state (un-locked)
        foreach (var colorOption in dataSO.colors)
        {
            if (currentSkinShopData.colorsLockedState.TryGetValue(colorOption.Id, out bool locked))
            {
                colorOption.isLocked = locked;
            }
            else // new color added => locked by default
            {
                colorOption.isLocked = true;
            }
        }
    }

    /// <summary>
    /// Restores the Tutorial info <br/>
    /// (Tutorial1, Shop, Rocket, Ufo)
    /// </summary>
    /// <param name="fileName"></param>
    private void RestoreTutorialDataFromFile(string fileName)
    {
        if (TutorialManager.Instance == null) return;

        currentTutorialData = LoadFile<TutorialData>(fileName);

        if (currentTutorialData == null)
        {
            currentTutorialData = new TutorialData()
            {
                isTutorial1Complete = false,
                isTutorialShopComplete = false,
                isTutorialRocketComplete = false,
                isTutorialUfoComplete = false,
                wasCoinsReceived = false,
                wasRocketReceived = false,
                wasUfoReceived = false,
            };
        }

        TutorialManager.Instance.IsTutorial1Complete = currentTutorialData.isTutorial1Complete;
        TutorialManager.Instance.IsTutorialShopComplete = currentTutorialData.isTutorialShopComplete;
        TutorialManager.Instance.IsTutorialRocketComplete = currentTutorialData.isTutorialRocketComplete;
        TutorialManager.Instance.IsTutorialUfoComplete = currentTutorialData.isTutorialUfoComplete;
        CoinManager.Instance.WasCoinsReceived = currentTutorialData.wasCoinsReceived;
        CoinManager.Instance.WasRocketReceived = currentTutorialData.wasRocketReceived;
        CoinManager.Instance.WasUfoReceived = currentTutorialData.wasUfoReceived;

        SaveDataInFile(currentTutorialData, fileName);
    }


    // --- Helper functions ---

    private T LoadFile<T>(string fileName) where T : SaveableData
    {
        if (string.IsNullOrEmpty(fileName))
        {
            //Debug.LogError("Session name cannot be empty.");
            return null;
        }

        T loadedData = dataService.Load<T>(fileName);
        if (loadedData == null)
        {
            //Debug.LogWarning($"No file named \'{fileName}\' found.");
            return null;
        }

        return loadedData;
    }
    private void SaveDataInFile(SaveableData data, string fileName, bool writeOverride = true)
    {
        if (dataService.Save(data, fileName, writeOverride))
        {
            //Debug.Log($"\'{fileName}\' saved successfully.");
        }
        else
        {
            //Debug.LogError($"Failed to save \'{fileName}\'\n");
        }
    }
}


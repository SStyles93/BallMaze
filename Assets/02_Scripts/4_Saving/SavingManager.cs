using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class SavingManager : MonoBehaviour
{
    public static SavingManager Instance { get; private set; }

    private IDataService dataService;

    public GameData currentGameData = null;
    public PlayerData currentPlayerData = null;
    public CustomizationShopData currentShopData = null;
    public SettingsData currentSettingsData = null;

    const string GameDataFileName = "GameData";
    const string PlayerDataFileName = "PlayerData";
    const string ShopDataFileName = "ShopData";
    const string SettingsDataFileName = "SettingsData";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

        dataService = new JsonDataService(); // Or any other IDataService implementation
    }

    // --- SAVE ---

    /// <summary>
    /// Saves the session in a Session.Json file
    /// </summary>
    public void SaveSession()
    {
        SaveGameDataInFile(GameDataFileName);

        SavePlayerDataInFile(PlayerDataFileName);

        SaveCustomizationShopDataInFile(ShopDataFileName);

        SaveSettingsDataInFile(SettingsDataFileName);

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
    /// <summary>
    /// Captures the player's data (currency (int), colorIndex(int), materialIndex(int))
    /// </summary>
    public void SavePlayer()
    {
        SavePlayerDataInFile(PlayerDataFileName);
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
    public void SaveShop()
    {
        SaveCustomizationShopDataInFile(ShopDataFileName);
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


    /// <summary>
    /// Saves the TimeStamp & Levels in a "GameData.json" files
    /// </summary>
    private void SaveGameDataInFile(string fileName)
    {
        if(currentGameData == null)
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

        foreach (var kvp in LevelManager.Instance.LevelDataDictionnary)
        {
            if (gameData.levelsData.ContainsKey(kvp.Key))
                gameData.levelsData[kvp.Key] = kvp.Value;
            else
                gameData.levelsData.Add(kvp.Key, kvp.Value);
        }
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
        playerSaveData.coins = coinManager.CoinAmount;
        playerSaveData.stars = coinManager.StarAmount;
        playerSaveData.hearts = coinManager.HeartAmount;
        playerSaveData.lastHeartRefillTime = coinManager.LastHeartRefillTime;


        // --- SHOP ---
        ShopManager:
        CustomizationManager shopManager = CustomizationManager.Instance;
        if (shopManager == null)
        {
            //Debug.Log("No ShopManager instance available");
            return;
        }
        playerSaveData.colorIndex = shopManager.skinData_SO.playerColorIndex;
        playerSaveData.materialIndex = shopManager.skinData_SO.playerMaterialIndex;


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

        CustomizationShopData shopData = new CustomizationShopData
        {
            colorsLockedState = new List<bool>(CustomizationManager.Instance.customizationData_SO.colors.Length),
            materialsLockedState = new List<bool>(CustomizationManager.Instance.customizationData_SO.colors.Length)
        };

        // Save colors state (un-locked)
        foreach (var colorOption in currentDataSO.colors)
        {
            shopData.colorsLockedState.Add(colorOption.isLocked);
        }

        // Save materials state (un-locked)
        foreach (var materialOption in currentDataSO.materials)
        {
            shopData.materialsLockedState.Add(materialOption.isLocked);
        }

        currentShopData = shopData;

        SaveDataInFile(currentShopData, fileName);
    }



    // --- LOAD ---

    /// <summary>
    /// Loads the session from a Session.Json file
    /// </summary>
    public void LoadSession()
    {
        RestoreGameDataFromFile(GameDataFileName);

        RestorePlayerDataFromFile(PlayerDataFileName);

        RestoreShopDataFromFile(ShopDataFileName);

        RestoreSettingsDataFromFile(SettingsDataFileName);
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
        RestoreShopDataFromFile(ShopDataFileName);
    }
    public void LoadSettings()
    {
        RestoreSettingsDataFromFile(SettingsDataFileName);
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

        LevelManager.Instance.LevelDataDictionnary.Clear();

        if (gameData.levelsData == null)
        {
            LevelData levelsData = new LevelData();

            //Debug.Log("Current Session Data does not exist, creating new levelsData");
        }
        else
        {
            // --- DEBUG: Creates "finished" leveldata for all levels
            if (CoreManager.Instance.unlockAllLevels)
            {
                LevelData levelData = new LevelData()
                {
                    numberOfStars = 3,
                    currencyLeftToEarn = 0,
                    wasLevelFinished = true
                };
                for (int i = 0; i <= CoreManager.Instance.numberOfLevels; i++)
                {
                    LevelManager.Instance.LevelDataDictionnary.Add(i, levelData);
                }
            }
            else
            {
                // Retrieves the LevelDatas and sets them in the LevelManager
                foreach (var kvp in gameData.levelsData)
                {
                    LevelManager.Instance.LevelDataDictionnary[kvp.Key] = kvp.Value;
                }
            }
        }
    }

    /// <summary>
    /// Restores the data for the player (currency (int))
    /// </summary>
    private void RestorePlayerDataFromFile(string fileName)
    {
        currentPlayerData = LoadFile<PlayerData>(PlayerDataFileName);

        if (currentPlayerData == null)
        {
            PlayerData playerData = new PlayerData()
            {
                lastHeartRefillTime = DateTime.UtcNow,
                coins = 0,
                stars = 0,
                hearts = CoinManager.Instance.InitialHeartAmount,
                colorIndex = 0,
                materialIndex = 0,

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
        coinManager.SetCurrencyAmount(CoinType.STAR, currentPlayerData.stars);
        coinManager.SetCurrencyAmount(CoinType.HEART, currentPlayerData.hearts);
        coinManager.SetLastHeartRefillTime(currentPlayerData.lastHeartRefillTime);
        LifeManager.Instance.ResetLife();
        

        // --- SHOP ---
        ShopManager:
        CustomizationManager shopManager = CustomizationManager.Instance;
        if (shopManager == null)
        {
            //Debug.Log("No ShopManager instance available");
            return;
        }
        shopManager.skinData_SO.playerColor = shopManager.customizationData_SO.colors[currentPlayerData.colorIndex].color;
        shopManager.skinData_SO.playerMaterial = shopManager.customizationData_SO.materials[currentPlayerData.materialIndex].material;
        shopManager.skinData_SO.playerColorIndex = currentPlayerData.colorIndex;
        shopManager.skinData_SO.playerMaterialIndex = currentPlayerData.materialIndex;
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
            return;
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
    private void RestoreShopDataFromFile(string fileName)
    {
        if (CustomizationManager.Instance == null) return;

        currentShopData = LoadFile<CustomizationShopData>(ShopDataFileName);

        var dataSO = CustomizationManager.Instance.customizationData_SO;

        if (currentShopData == null)
        {
            //Debug.Log("Current ShopData does not exist, creating a new one");
            currentShopData = new CustomizationShopData();
            return;
        }

        // Restore colors state (un-locked)
        for (int i = 0; i < dataSO.colors.Count(); i++)
        {
            // Ensure saved list has the same size
            if (i < currentShopData.colorsLockedState.Count)
                dataSO.colors[i].isLocked = currentShopData.colorsLockedState[i];
        }

        // Restore materials state (un-locked)
        for (int i = 0; i < dataSO.materials.Count(); i++)
        {
            if (i < currentShopData.materialsLockedState.Count)
                dataSO.materials[i].isLocked = currentShopData.materialsLockedState[i];
        }
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


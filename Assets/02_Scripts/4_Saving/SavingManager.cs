using System;
using UnityEditor;
using UnityEngine;

public class SavingManager : MonoBehaviour
{
    public static SavingManager Instance { get; private set; }

    private IDataService dataService;

    private GameData currentGameData = null;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

        dataService = new JsonDataService(); // Or any other IDataService implementation
    }

    // ---------------- SAVE ----------------
    public void SaveSession()
    {
        currentGameData = new GameData
        {
            timestamp = DateTime.Now,
        };

        // 1. Save Player Data (currency for the moment)
        CapturePlayerData();

        CaptureLevelsData();

        // 2. Save file changes
        SaveFile("Session");
    }

    private void CapturePlayerData()
    {
        PlayerData playerSaveData = new PlayerData
        {
            currency = CurrencyManager.Instance.currencyValue
        };

        currentGameData.playerData = playerSaveData;
    }

    private void CaptureLevelsData()
    {
        foreach (var kvp in LevelManager.Instance.KvpLevelData)
        {
            if (currentGameData.levelsData.ContainsKey(kvp.Key))
                currentGameData.levelsData[kvp.Key] = kvp.Value;
            else
                currentGameData.levelsData.Add(kvp.Key, kvp.Value);
        }
    }


    // ---------------- LOAD ----------------

    /// <summary>
    /// Loads the GameData and calls restore for player and levels data
    /// </summary>
    public void LoadSession()
    {
        currentGameData = LoadFile("Session");
        if (currentGameData == null)
        {
            Debug.Log("Current Session Data does not exist, creating new GameData");
            currentGameData = new GameData();
        }

        RestorePlayerData();

        RestoreLevelsData();
    }

    /// <summary>
    /// Restores the data for the player
    /// </summary>
    private void RestorePlayerData()
    {
        if(currentGameData.playerData == null)
        {
            PlayerData playerData = new PlayerData()
            {
                currency = 0
            };
            Debug.Log("Current Session Data does not exist, creating new PlayerData");
            currentGameData.playerData = playerData;
        }

        CurrencyManager.Instance.currencyValue = currentGameData.playerData.currency;
    }

    /// <summary>
    /// Restores the data for all levels
    /// </summary>
    private void RestoreLevelsData()
    {
        if (LevelManager.Instance == null) return;
        
        LevelManager.Instance.KvpLevelData.Clear();

        if(currentGameData.levelsData == null)
        {
            LevelData levelsData = new LevelData()
            {

            };
            Debug.Log("Current Session Data does not exist, creating new levelsData");
        }
        else
        {
            foreach (var kvp in currentGameData.levelsData)
            {
                LevelManager.Instance.KvpLevelData[kvp.Key] = kvp.Value;
            }
        }
    }


    // ------------ Helper functions ------------


    public GameData LoadFile(string sessionID)
    {
        if (string.IsNullOrEmpty(sessionID))
        {
            Debug.LogError("Session name cannot be empty.");
            return null;
        }

        GameData loadedData = dataService.Load<GameData>(sessionID);
        if (loadedData == null)
        {
            Debug.LogWarning($"No session \'{sessionID}\' found.");
            return null;
        }

        return loadedData;
    }

    private void SaveFile(string sessionID, bool writeOverride = true)
    {
        if (dataService.Save(currentGameData, sessionID, writeOverride))
        {
            Debug.Log($"Session \'{sessionID}\' saved successfully.");
        }
        else
        {
            Debug.LogError($"Failed to save session \'{sessionID}\'\n");
        }
    }
}


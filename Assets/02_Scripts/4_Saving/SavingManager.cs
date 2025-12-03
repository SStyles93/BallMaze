using System;
using System.Collections.Generic;
using System.Linq;
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

    /// <summary>
    /// Saves the session in a Session.Json file
    /// </summary>
    public void SaveSession()
    {
        currentGameData = new GameData
        {
            timestamp = DateTime.Now,
        };

        CapturePlayerData();

        CaptureLevelsData();

        CaptureShopData();

        SaveFile("Session");
    }

    /// <summary>
    /// Captures the player's data (currency (int))
    /// </summary>
    private void CapturePlayerData()
    {
        if(CurrencyManager.Instance == null)
        {
            Debug.Log("No CurrencyManager instance available");
            return;
        }

        PlayerData playerSaveData = new PlayerData
        {
            currency = CurrencyManager.Instance.currencyValue,
            colorIndex = ShopManager.Instance.skinData_SO.playerColorIndex,
            materialIndex = ShopManager.Instance.skinData_SO.playerMaterialIndex
        };

        currentGameData.playerData = playerSaveData;
    }

    /// <summary>
    /// Captures the data of all levels (lvl n° (int), grade (int), score (int))
    /// </summary>
    private void CaptureLevelsData()
    {
        if (LevelManager.Instance == null)
        {
            Debug.Log("No LevelManager instance available");
            return;
        }

        foreach (var kvp in LevelManager.Instance.KvpLevelData)
        {
            if (currentGameData.levelsData.ContainsKey(kvp.Key))
                currentGameData.levelsData[kvp.Key] = kvp.Value;
            else
                currentGameData.levelsData.Add(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Method to capture the state of customization options
    /// </summary>
    private void CaptureShopData()
    {
        if (ShopManager.Instance == null) return;

        CustomizationData_SO currentDataSO = ShopManager.Instance.customizationData_SO;

        ShopData shopData = new ShopData
        {
            colorsLockedState = new List<bool>(ShopManager.Instance.customizationData_SO.colors.Length),
            materialsLockedState = new List<bool>(ShopManager.Instance.customizationData_SO.colors.Length)
        };

        // Save colors state (un-locked
        foreach (var colorOption in currentDataSO.colors)
        {
            shopData.colorsLockedState.Add(colorOption.isLocked);
        }

        // Save materials state (un-locked)
        foreach (var materialOption in currentDataSO.materials)
        {
            shopData.materialsLockedState.Add(materialOption.isLocked);
        }

        currentGameData.shopData = shopData;
    }


    // ---------------- LOAD ----------------

    /// <summary>
    /// Loads the session from a Session.Json file
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

        RestoreShopData();
    }

    /// <summary>
    /// Restores the data for the player (currency (int))
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
    /// Restores the data for all levels (lvl n°, grade(int), score(int)
    /// </summary>
    private void RestoreLevelsData()
    {
        if (LevelManager.Instance == null) return;
        
        LevelManager.Instance.KvpLevelData.Clear();

        if(currentGameData.levelsData == null)
        {
            LevelData levelsData = new LevelData(){};

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

    /// <summary>
    /// Restores all the customization states from the ShopData (color.isLocked & material.isLocked)
    /// </summary>
    private void RestoreShopData()
    {
        if (ShopManager.Instance == null) return;

        var dataSO = ShopManager.Instance.customizationData_SO;

        if (currentGameData?.shopData == null)
        {
            Debug.Log("Current ShopData does not exist, creating a new one");
            currentGameData.shopData = new ShopData();
            return;
        }

        // Restore colors state (un-locked)
        for (int i = 0; i < dataSO.colors.Count(); i++)
        {
            // Ensure saved list has the same size
            if (i < currentGameData.shopData.colorsLockedState.Count)
                dataSO.colors[i].isLocked = currentGameData.shopData.colorsLockedState[i];
        }

        // Restore materials state (un-locked)
        for (int i = 0; i < dataSO.materials.Count(); i++)
        {
            if (i < currentGameData.shopData.materialsLockedState.Count)
                dataSO.materials[i].isLocked = currentGameData.shopData.materialsLockedState[i];
        }
    }


    // ------------ Helper functions ------------

    private GameData LoadFile(string sessionID)
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

    private void SaveFile(string sessionName, bool writeOverride = true)
    {
        if (dataService.Save(currentGameData, sessionName, writeOverride))
        {
            Debug.Log($"Session \'{sessionName}\' saved successfully.");
        }
        else
        {
            Debug.LogError($"Failed to save session \'{sessionName}\'\n");
        }
    }
}


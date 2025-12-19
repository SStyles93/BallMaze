using System;
using System.Collections.Generic;

public class SaveableData { }

/// <summary>
/// Data of the Session (Game)
/// </summary>
[System.Serializable]
public class GameData : SaveableData
{
    public DateTime firstTimestamp; // Date-Time at which the game was first launched
    public Dictionary<int, LevelData> levelsData = new Dictionary<int, LevelData>(); // The dictionary holding the kvp sceneID - LevelData
}

/// <summary>
/// The data for one scene "level"
/// </summary>
[System.Serializable]
public class LevelData
{
    public int numberOfStars;
    public int currencyLeftToEarn;
    public bool wasLevelFinished = false;
}

/// <summary>
/// Data of the player (coins, stars, hearts, colorIndex, materialIndex)
/// </summary>
[System.Serializable]
public class PlayerData : SaveableData
{
    public DateTime lastHeartRefillTime; // Date-Time at which the game was first launched
    public int coins;
    public int stars;
    public int hearts;
    public int colorIndex;
    public int materialIndex;
}

/// <summary>
/// The data of the customization menu options (locked or not)
/// </summary>
[System.Serializable]
public class CustomizationShopData : SaveableData
{
    public List<bool> colorsLockedState;
    public List<bool> materialsLockedState;
}

/// <summary>
/// The state of the settings
/// </summary>
[System.Serializable]
public class SettingsData : SaveableData
{
    public bool isAudioOn = true;
    public bool isMusicOn = true;
    public bool isVibrationOn = true;
}

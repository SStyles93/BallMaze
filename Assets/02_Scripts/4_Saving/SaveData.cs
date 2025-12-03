using System;
using System.Collections.Generic;

/// <summary>
/// Data of the player (currency)
/// </summary>
[System.Serializable]
public class PlayerData
{
    public int currency;
    public int colorIndex;
    public int materialIndex;
}

/// <summary>
/// The data for one scene "level"
/// </summary>
[System.Serializable]
public class LevelData
{
    public int levelGrade;
    public int levelScore;
    public int currencyLeftToEarn;
}

/// <summary>
/// The data of the customization menu options (locked or not)
/// </summary>
[System.Serializable]
public class ShopData
{
    public List<bool> colorsLockedState;
    public List<bool> materialsLockedState;
}

/// <summary>
/// Data of the Session (Game)
/// </summary>
[System.Serializable]
public class GameData
{
    public DateTime timestamp; // Date-Time at which the game was saved
    public PlayerData playerData;
    public Dictionary<int, LevelData> levelsData = new Dictionary<int, LevelData>(); // The dictionary holding the kvp sceneID - LevelData
    public ShopData shopData;
    public string lastUnlockedScene; // The current unlocked scene when saving
}

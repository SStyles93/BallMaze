using System;
using System.Collections.Generic;

/// <summary>
/// Data of the player (currency)
/// </summary>
[System.Serializable]
public class PlayerData
{
    public int currency;
}

/// <summary>
/// The data for one scene "level"
/// </summary>
[System.Serializable]
public class LevelData
{
    public int levelGrade;
    public int levelScore;
    public float levelTime;
}

/// <summary>
/// Data of the Session (Game)
/// </summary>
[System.Serializable]
public class GameData
{
    public DateTime timestamp; // Date-Time at which the game was saved
    public PlayerData playerData; // The data of the player
    public Dictionary<int, LevelData> levelsData = new Dictionary<int, LevelData>(); // The dictionary holding the kvp sceneID - LevelData
    public string lastUnlockedScene; // The current unlocked scene when saving
}

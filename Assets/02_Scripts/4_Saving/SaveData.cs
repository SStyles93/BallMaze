using JetBrains.Annotations;
using System;
using System.Collections.Generic;

// --- Main Save File Structure ---

[System.Serializable]
public class PlayerSaveData
{
    int currentSceneUnlocked;
    int currency;
}

/// <summary>
/// The root container for the entire scene save data.
/// </summary>
[System.Serializable]
public class SceneSaveData
{
    int sceneID;
    int sceneScore;
}

[System.Serializable]
public class SessionSaveData
{
    public DateTime timestamp; // Date-Time at which the game was saved
    public PlayerSaveData playerData; // The data of the player
    public Dictionary<string, SceneSaveData> sceneData = new Dictionary<string, SceneSaveData>(); // The dictionary holding the kvp sceneName - SceneSave Data
    public string currentSceneName; // The current active scene when saving
}





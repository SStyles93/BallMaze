using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject cameraPrefab;

    private IDataService dataService;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

        dataService = new JsonDataService(); // Or any other IDataService implementation
    }


    // ---------------- SAVE ----------------
    public void SaveSession(string sessionID)
    {
        if (string.IsNullOrEmpty(sessionID))
        {
            Debug.LogError("Session name cannot be empty.");
            return;
        }

        // 1. Save Player Data
        CapturePlayerData();

        // 2. Save all loaded scenes data

        // 3. Save file changes
        SaveFile(sessionID);
    }

    private void CapturePlayerData()
    {
    }


    // ---------------- LOAD ----------------
    // ------------ RESTORE ON CALLBACK ------------


    //public SessionSaveData GetSessionFileInfo(string sessionName)
    //{
    //    if (string.IsNullOrEmpty(sessionName))
    //    {
    //        Debug.LogError("Session name cannot be empty.");
    //        return null;
    //    }

    //    //SessionSaveData loadedData = dataService.Load<SessionSaveData>(GetSessionFileName(sessionName));
    //    //if (loadedData == null)
    //    //{
    //    //    Debug.LogWarning($"No session \'{sessionName}\' found.");
    //    //    return null;
    //    //}

    //    //return loadedData;
    //}

    //private string GetSessionFileName(string sessionName)
    //{
    //}

    private void SaveFile(string sessionID, bool writeOverride = true)
    {
        // 3. Save the session file
        //if (dataService.Save(currentSessionData, GetSessionFileName(sessionID), writeOverride))
        //{
        //    Debug.Log($"Session \'{sessionID}\' saved successfully.");
        //}
        //else
        //{
        //    Debug.LogError($"Failed to save session \'{sessionID}\'\n");
        //}
    }
}


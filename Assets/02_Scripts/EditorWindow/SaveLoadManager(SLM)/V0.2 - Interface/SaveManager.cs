//using UnityEngine;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using UnityEditor;

//public static class SaveManager
//{
//    // Delegate for objects to subscribe to when they need to save their data
//    public delegate void OnSaveRequestDelegate();
//    public static event OnSaveRequestDelegate OnSaveRequest;

//    // Delegate for objects to subscribe to when they need to load their data
//    public delegate void OnLoadRequestDelegate();
//    public static event OnLoadRequestDelegate OnLoadRequest;

//    private const string SAVE_FILE_NAME = "gameSaveData.json";

//    // Call this method to trigger all subscribed objects to save their data
//    public static void TriggerSave()
//    {
//        Debug.Log("SaveManager: Triggering Save Request...");
//        if (OnSaveRequest != null)
//        {
//            OnSaveRequest.Invoke();
//            Debug.Log("SaveManager: Save Request Invoked.");
//        }
//        else
//        {
//            Debug.LogWarning("SaveManager: No objects subscribed to OnSaveRequest.");
//        }
//        SaveAllSavableObjects();
//    }

//    // Call this method to trigger all subscribed objects to load their data
//    public static void TriggerLoad()
//    {
//        Debug.Log("SaveManager: Triggering Load Request...");
//        LoadAllSavableObjects();
//        if (OnLoadRequest != null)
//        {
//            OnLoadRequest.Invoke();
//            Debug.Log("SaveManager: Load Request Invoked.");
//        }
//        else
//        {
//            Debug.LogWarning("SaveManager: No objects subscribed to OnLoadRequest.");
//        }
//    }

//    // Internal method to collect and save data from all ISavable objects in the scene
//    private static void SaveAllSavableObjects()
//    {
//        Dictionary<string, SavedObjectData> allSavedData = new Dictionary<string, SavedObjectData>();

//        // Find all active ISavable objects in the scene
//        ISavable[] savableObjects = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID).OfType<ISavable>().ToArray();

//        foreach (ISavable savable in savableObjects)
//        {
//            string id = savable.GetSavableID();
//            if (string.IsNullOrEmpty(id))
//            {
//                Debug.LogWarning($"SaveManager: ISavable object {((MonoBehaviour)savable).name} has no ID and will not be saved.");
//                continue;
//            }

//            try
//            {
//                object data = savable.GetSaveData();
//                if (data != null)
//                {
//                    // Use AssemblyQualifiedName to ensure correct type resolution during loading
//                    string typeName = data.GetType().AssemblyQualifiedName;
//                    string jsonData = JsonUtility.ToJson(data);

//                    allSavedData[id] = new SavedObjectData
//                    {
//                        savableID = id,
//                        dataType = typeName,
//                        jsonContent = jsonData
//                    };
//                }
//                else
//                {
//                    Debug.LogWarning($"SaveManager: GetSaveData() for {id} returned null. Not saving data for this object.");
//                }
//            }
//            catch (Exception e)
//            {
//                Debug.LogError($"SaveManager: Error saving data for {id}: {e.Message}");
//            }
//        }

//        string fullPath = Application.dataPath + "/" + SAVE_FILE_NAME;
//        try
//        {
//            // Wrap the dictionary in a serializable class for JsonUtility
//            string jsonToSave = JsonUtility.ToJson(new SerializableDictionary<string, SavedObjectData>(allSavedData), true);
//            File.WriteAllText(fullPath, jsonToSave);
//            Debug.Log($"SaveManager: All savable data written to {fullPath}");
//        }
//        catch (Exception e)
//        {
//            Debug.LogError($"SaveManager: Failed to write save file to {fullPath}: {e.Message}");
//        }

//        AssetDatabase.Refresh();
//    }

//    // Internal method to load data and distribute it to ISavable objects
//    private static void LoadAllSavableObjects()
//    {
//        string fullPath = Application.dataPath + "/" + SAVE_FILE_NAME;
//        if (!File.Exists(fullPath))
//        {
//            Debug.LogWarning($"SaveManager: Save file not found at {fullPath}. Nothing to load.");
//            return;
//        }

//        try
//        {
//            string jsonToLoad = File.ReadAllText(fullPath);
//            // Deserialize the wrapper first
//            SerializableDictionary<string, SavedObjectData> loadedData = JsonUtility.FromJson<SerializableDictionary<string, SavedObjectData>>(jsonToLoad);

//            ISavable[] savableObjects = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID).OfType<ISavable>().ToArray();

//            foreach (ISavable savable in savableObjects)
//            {
//                string id = savable.GetSavableID();
//                if (string.IsNullOrEmpty(id))
//                {
//                    Debug.LogWarning($"SaveManager: ISavable object {((MonoBehaviour)savable).name} has no ID and will not be loaded.");
//                    continue;
//                }

//                if (loadedData.TryGetValue(id, out SavedObjectData dataToLoad))
//                {
//                    try
//                    {
//                        System.Type dataType = System.Type.GetType(dataToLoad.dataType);
//                        if (dataType != null)
//                        {
//                            object deserializedData = JsonUtility.FromJson(dataToLoad.jsonContent, dataType);
//                            savable.LoadSaveData(deserializedData);
//                        }
//                        else
//                        {
//                            Debug.LogError($"SaveManager: Could not find type {dataToLoad.dataType} for object {id} during load.");
//                        }
//                    }
//                    catch (Exception e)
//                    {
//                        Debug.LogError($"SaveManager: Error loading data for {id}: {e.Message}");
//                    }
//                }
//                else
//                {
//                    Debug.LogWarning($"SaveManager: No saved data found for object with ID: {id}");
//                }
//            }
//            Debug.Log($"SaveManager: All savable data loaded from {fullPath}");
//        }
//        catch (Exception e)
//        {
//            Debug.LogError($"SaveManager: Failed to read save file from {fullPath}: {e.Message}");
//        }
//    }
//}

//// Helper class to store data for each savable object
//[System.Serializable]
//public class SavedObjectData
//{
//    public string savableID;
//    public string dataType; // AssemblyQualifiedName of the data type
//    public string jsonContent; // JSON string of the actual data
//}

//// Re-using the SerializableDictionary from previous task for internal use
//[System.Serializable]
//public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
//{
//    [SerializeField] private List<TKey> keys = new List<TKey>();
//    [SerializeField] private List<TValue> values = new List<TValue>();

//    public SerializableDictionary() : base() { }
//    public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }

//    public void OnBeforeSerialize()
//    {
//        keys.Clear();
//        values.Clear();
//        foreach (KeyValuePair<TKey, TValue> pair in this)
//        {
//            keys.Add(pair.Key);
//            values.Add(pair.Value);
//        }
//    }

//    public void OnAfterDeserialize()
//    {
//        this.Clear();
//        if (keys.Count != values.Count)
//        {
//            Debug.LogError("There are " + keys.Count + " keys and " + values.Count + " values after deserialization. Make sure that both key and value types are serializable.");
//        }
//        else
//        {
//            for (int i = 0; i < keys.Count; i++)
//            {
//                this.Add(keys[i], values[i]);
//            }
//        }
//    }
//}



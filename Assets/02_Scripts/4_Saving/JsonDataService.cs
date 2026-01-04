using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class JsonDataService : IDataService
{
    public bool Save<T>(T data, string fileName, bool overwrite)
    {
        string path = GetFilePath(fileName);

        if (Directory.Exists(GetFolderPath("Data")))
        {
            // The folder exists
            Debug.Log("Folder found: " + GetFolderPath("Data"));
        }
        else
        {
            // The folder does not exist
            Debug.Log("Folder not found: " + GetFolderPath("Data"));
            // You can create it if needed:
            Directory.CreateDirectory(GetFolderPath("Data"));
        }

        try
        {

            if (File.Exists(path) && !overwrite)
            {
                Debug.LogError($"File already exists and overwrite is false: {path}");
                return false;
            }

            // Use JsonConvert from Newtonsoft.Json
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
#if UNITY_EDITOR
            //AssetDatabase.Refresh();
#endif
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Could not save data to {path}: {e.Message}");
            return false;
        }
    }

    public T Load<T>(string fileName)
    {
        string path = GetFilePath(fileName);
        try
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"File not found: {path}");
                return default(T);
            }

            string json = File.ReadAllText(path);
            // Use JsonConvert from Newtonsoft.Json
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Could not load data from {path}: {e.Message}");
            return default(T);
        }
    }

    public void Delete(string fileName)
    {
        string path = GetFilePath(fileName);
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Could not delete file {path}: {e.Message}");
        }
    }

    public void ClearAllData()
    {
        Debug.LogWarning("ClearAllData not fully implemented. Implement with caution.");
    }

    private string GetFilePath(string fileName)
    {
#if UNITY_EDITOR
        return Path.Combine(GetFolderPath("Data"), $"{fileName}.json");
#else
        return Path.Combine(GetFolderPath("Data"), $"{fileName}.json");
#endif
    }

    private string GetFolderPath(string folderName)
    {
#if UNITY_EDITOR
        return Path.Combine(Application.dataPath, folderName);
#else
        return Path.Combine(Application.persistentDataPath, folderName);
#endif
    }
}

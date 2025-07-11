using UnityEngine; 
using System.IO; 
using System.Runtime.Serialization.Formatters.Binary; 
using System.Collections.Generic; 

public static class SaveLoadManager 
{ 
    public static void SaveData<T>(T data, string fileName) 
    { 
        string path = Application.persistentDataPath + "/" + fileName + ".json"; 
        string json = JsonUtility.ToJson(data, true); 
        File.WriteAllText(path, json); 
        Debug.Log("Data saved to: " + path); 
    } 

    public static T LoadData<T>(string fileName) 
    { 
        string path = Application.persistentDataPath + "/" + fileName + ".json"; 
        if (File.Exists(path)) 
        { 
            string json = File.ReadAllText(path); 
            T data = JsonUtility.FromJson<T>(json); 
            Debug.Log("Data loaded from: " + path); 
            return data; 
        } 
        else 
        { 
            Debug.LogError("Save file not found in " + path); 
            return default(T); 
        } 
    } 
}




[System.Serializable]
public class PlayerData
{
    public string playerName;
    public int health;
    public float[] position;

    public PlayerData(string name, int hp, Vector3 pos)
    {
        playerName = name;
        health = hp;
        position = new float[] { pos.x, pos.y, pos.z };
    }
}



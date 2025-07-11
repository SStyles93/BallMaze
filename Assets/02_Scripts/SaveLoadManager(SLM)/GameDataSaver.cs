using UnityEngine;

public class GameDataSaver : MonoBehaviour
{
    public MyCustomComponent targetComponent;
    public string saveFileName = "myGameObjectSave";

    void Start()
    {
        if (targetComponent == null)
        {
            targetComponent = GetComponent<MyCustomComponent>();
            if (targetComponent == null)
            {
                Debug.LogError("MyCustomComponent not found on this GameObject. Please attach one or assign it.");
                enabled = false; // Disable this script if no component to save
                return;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveGameObjectData();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadGameObjectData();
        }
    }

    public void SaveGameObjectData()
    {
        if (targetComponent != null)
        {
            MyCustomComponentData data = targetComponent.GetSaveData();
            SaveLoadManager.SaveData(data, saveFileName);
            Debug.Log("GameObject data saved.");
        }
    }

    public void LoadGameObjectData()
    {
        if (targetComponent != null)
        {
            MyCustomComponentData data = SaveLoadManager.LoadData<MyCustomComponentData>(saveFileName);
            if (data != null)
            {
                targetComponent.LoadSaveData(data);
                Debug.Log("GameObject data loaded.");
            }
        }
    }
}



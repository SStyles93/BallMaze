//using UnityEngine;

//public class MyCustomComponent : MonoBehaviour, ISavable
//{
//    public string componentName = "Default Component";
//    public float value = 10.5f;
//    public bool isActive = true;
//    public Vector3 customPosition = Vector3.zero;

//    // Unique ID for this savable object. Could be a GUID generated on Start() if not set in editor.
//    public string savableID = System.Guid.NewGuid().ToString(); 

//    // Implement ISavable.GetSavableID()
//    public string GetSavableID()
//    {
//        return savableID;
//    }

//    // Implement ISavable.GetSaveData()
//    public object GetSaveData()
//    {
//        return new MyCustomComponentData(componentName, value, isActive, customPosition);
//    }

//    // Implement ISavable.LoadSaveData()
//    public void LoadSaveData(object data)
//    {
//        if (data is MyCustomComponentData myData)
//        {
//            componentName = myData.componentName;
//            value = myData.value;
//            isActive = myData.isActive;
//            customPosition = new Vector3(myData.position[0], myData.position[1], myData.position[2]);
//        }
//        else
//        {
//            Debug.LogError($"Invalid data type for MyCustomComponent.LoadSaveData: {data.GetType()}");
//        }
//    }

//    void Start()
//    {
//        // If you want to ensure a unique ID even if not set in editor, uncomment this:
//        // if (string.IsNullOrEmpty(savableID))
//        // {
//        //     savableID = System.Guid.NewGuid().ToString();
//        // }
//        Debug.Log($"MyCustomComponent initialized (ID: {savableID}): Name={componentName}, Value={value}, Active={isActive}, Position={customPosition}");

//        // Subscribe to the SaveManager's OnLoadRequest to load data when the game starts or a load is triggered
//        SaveManager.OnLoadRequest += OnLoadRequestCallback;

//        // Example: Change value over time
//        if (Input.GetKeyDown(KeyCode.R))
//        {
//            value = Random.Range(0f, 100f);
//            Debug.Log($"MyCustomComponent value changed to: {value}");
//        }
//    }

//    void OnDestroy()
//    {
//        // Unsubscribe to prevent memory leaks
//        SaveManager.OnLoadRequest -= OnLoadRequestCallback;
//    }

//    private void OnLoadRequestCallback()
//    {
//        // When OnLoadRequest is triggered, the SaveManager will call LoadSaveData directly on this object
//        // So, this callback is mainly for any additional logic needed after loading, if any.
//        // For now, it just logs that a load request was received.
//        Debug.Log($"MyCustomComponent (ID: {savableID}) received OnLoadRequest callback.");
//    }
//}

//[System.Serializable]
//public class MyCustomComponentData
//{
//    public string componentName;
//    public float value;
//    public bool isActive;
//    public float[] position; // To save Vector3

//    public MyCustomComponentData(string name, float val, bool active, Vector3 pos)
//    {
//        componentName = name;
//        value = val;
//        isActive = active;
//        position = new float[] { pos.x, pos.y, pos.z };
//    }
//}



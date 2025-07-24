
//using UnityEngine;

//public class TestSavableObject : MonoBehaviour, ISavable
//{
//    public string objectName = "TestObject";
//    public int someValue = 0;
//    public string savableID; // Unique ID for this object

//    void Awake()
//    {
//        // Generate a unique ID if not already set (e.g., in the Inspector)
//        if (string.IsNullOrEmpty(savableID))
//        {
//            savableID = System.Guid.NewGuid().ToString();
//        }
//    }

//    void OnEnable()
//    {
//        // Subscribe to the SaveManager's OnSaveRequest delegate
//        SaveManager.OnSaveRequest += SaveMyData;
//        Debug.Log($"TestSavableObject (ID: {savableID}) subscribed to SaveManager.OnSaveRequest.");
//    }

//    void OnDisable()
//    {
//        // Unsubscribe to prevent memory leaks
//        SaveManager.OnSaveRequest -= SaveMyData;
//        Debug.Log($"TestSavableObject (ID: {savableID}) unsubscribed from SaveManager.OnSaveRequest.");
//    }

//    // Implement ISavable.GetSavableID()
//    public string GetSavableID()
//    {
//        return savableID;
//    }

//    // Implement ISavable.GetSaveData()
//    public object GetSaveData()
//    {
//        // Create a serializable data class for this object's data
//        return new TestObjectData(objectName, someValue, transform.position);
//    }

//    // Implement ISavable.LoadSaveData()
//    public void LoadSaveData(object data)
//    {
//        if (data is TestObjectData testData)
//        {
//            objectName = testData.objectName;
//            someValue = testData.someValue;
//            transform.position = new Vector3(testData.position[0], testData.position[1], testData.position[2]);
//            Debug.Log($"TestSavableObject (ID: {savableID}) loaded data: Name={objectName}, Value={someValue}, Pos={transform.position}");
//        }
//        else
//        {
//            Debug.LogError($"Invalid data type for TestSavableObject.LoadSaveData: {data.GetType()}");
//        }
//    }

//    // This method is called when SaveManager.OnSaveRequest is invoked
//    private void SaveMyData()
//    {
//        // The actual saving logic is handled by SaveManager.SaveAllSavableObjects()
//        // This callback is just to demonstrate that the delegate was triggered.
//        Debug.Log($"TestSavableObject (ID: {savableID}) received save request. Data will be collected by SaveManager.");
//    }

//    void Update()
//    {
//        // Example: Change some value over time or with input
//        if (Input.GetKeyDown(KeyCode.T))
//        {
//            someValue = Random.Range(0, 100);
//            Debug.Log($"TestSavableObject (ID: {savableID}) someValue changed to: {someValue}");
//        }
//    }
//}

//[System.Serializable]
//public class TestObjectData
//{
//    public string objectName;
//    public int someValue;
//    public float[] position;

//    public TestObjectData(string name, int val, Vector3 pos)
//    {
//        objectName = name;
//        someValue = val;
//        position = new float[] { pos.x, pos.y, pos.z };
//    }
//}



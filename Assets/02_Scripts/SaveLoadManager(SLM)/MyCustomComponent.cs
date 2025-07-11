using UnityEngine;

public class MyCustomComponent : MonoBehaviour
{
    public string componentName = "Default Component";
    public float value = 10.5f;
    public bool isActive = true;
    public Vector3 customPosition = Vector3.zero;

    // This method will be called to get the data to save
    public MyCustomComponentData GetSaveData()
    {
        return new MyCustomComponentData(componentName, value, isActive, customPosition);
    }

    // This method will be called to load the data back into the component
    public void LoadSaveData(MyCustomComponentData data)
    {
        componentName = data.componentName;
        value = data.value;
        isActive = data.isActive;
        customPosition = new Vector3(data.position[0], data.position[1], data.position[2]);
    }

    void Start()
    {
        Debug.Log($"MyCustomComponent initialized: Name={componentName}, Value={value}, Active={isActive}, Position={customPosition}");
    }

    void Update()
    {
        // Example: Change value over time
        if (Input.GetKeyDown(KeyCode.R))
        {
            value = Random.Range(0f, 100f);
            Debug.Log($"MyCustomComponent value changed to: {value}");
        }
    }
}

[System.Serializable]
public class MyCustomComponentData
{
    public string componentName;
    public float value;
    public bool isActive;
    public float[] position; // To save Vector3

    public MyCustomComponentData(string name, float val, bool active, Vector3 pos)
    {
        componentName = name;
        value = val;
        isActive = active;
        position = new float[] { pos.x, pos.y, pos.z };
    }
}



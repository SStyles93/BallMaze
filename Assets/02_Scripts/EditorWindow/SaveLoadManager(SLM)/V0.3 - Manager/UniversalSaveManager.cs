using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;
using System.Linq;
using UnityEditor;

// Main data structure to hold all saved game data
[System.Serializable]
public class UniversalSaveData
{
    public List<GameObjectSaveData> gameObjects = new List<GameObjectSaveData>();
}

// Data structure for a single GameObject
[System.Serializable]
public class GameObjectSaveData
{
    public string name; // Using name for now, unique ID is better for robust loading
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 localScale;
    public List<ComponentSaveData> components = new List<ComponentSaveData>();
}

// Data structure for a single Component's data
[System.Serializable]
public class ComponentSaveData
{
    public string typeName; // Full type name of the component
    public string jsonValue; // JSON representation of the component's entire data (for built-in types)
    public List<FieldSaveData> fields = new List<FieldSaveData>(); // For custom MonoBehaviour fields
}

// Data structure for a single field's data
[System.Serializable]
public class FieldSaveData
{
    public string fieldName;
    public string fieldType; // Full type name of the field
    public string jsonValue; // JSON representation of the field's value
}

public class UniversalSaveManager : MonoBehaviour
{
    public List<GameObject> gameObjectsToSave = new List<GameObject>();
    public string saveFileName = "universalGameSave.json";

    private string SavePath => Application.dataPath + "/" + saveFileName;

    [ContextMenu("Save All GameObjects")]
    public void SaveAllGameObjects()
    {
        UniversalSaveData saveData = new UniversalSaveData();

        foreach (GameObject go in gameObjectsToSave)
        {
            if (go == null)
            {
                Debug.LogWarning("Skipping null GameObject in save list.");
                continue;
            }

            GameObjectSaveData goData = new GameObjectSaveData
            {
                name = go.name,
                position = go.transform.position,
                rotation = go.transform.rotation,
                localScale = go.transform.localScale
            };

            foreach (Component comp in go.GetComponents<Component>())
            {
                if (comp == null) continue;

                ComponentSaveData compData = new ComponentSaveData
                {
                    typeName = comp.GetType().AssemblyQualifiedName
                };


                BaseUnityComponentData data = new BaseUnityComponentData();
                switch (comp)
                {
                    case BoxCollider boxCollider:
                        data = new BoxColliderData
                        {
                            enabled = boxCollider.enabled,
                            center = boxCollider.center,
                            size = boxCollider.size,
                            isTrigger = boxCollider.isTrigger
                        };
                        compData.jsonValue = JsonUtility.ToJson(data);
                        break;

                    case MeshFilter meshFilter:
                        data = new MeshFilterData
                        {
                            enabled = meshFilter.sharedMesh != null,
                            meshName = meshFilter.sharedMesh?.name
                        };
                        compData.jsonValue = JsonUtility.ToJson(data);
                        break;
                    case MeshRenderer meshRenderer:

                        List<string> matNames = new List<string>();
                        foreach (Material mat in meshRenderer.sharedMaterials)
                        {
                            matNames.Add(mat.name);
                        }
                        data = new MeshRendererData
                        {
                            enabled = meshRenderer.enabled,
                            materialNames = matNames,
                            receiveShadows = meshRenderer.receiveShadows,
                            castShadows = (meshRenderer.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off)

                        };
                        compData.jsonValue = JsonUtility.ToJson(data);
                        break;

                }
                // Handle custom MonoBehaviour scripts using reflection for fields
                if (comp is MonoBehaviour || comp is ScriptableObject)
                {
                    FieldInfo[] fields = comp.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                                        .Where(f => f.IsPublic || f.IsDefined(typeof(SerializeField), true))
                                        .ToArray();

                    foreach (FieldInfo field in fields)
                    {
                        try
                        {
                            object value = field.GetValue(comp);
                            // JsonUtility.ToJson can only serialize objects that are serializable by Unity.
                            // For complex types, this might fail or produce empty JSON.
                            string jsonValue = JsonUtility.ToJson(value);

                            compData.fields.Add(new FieldSaveData
                            {
                                fieldName = field.Name,
                                fieldType = field.FieldType.AssemblyQualifiedName,
                                jsonValue = jsonValue
                            });
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Error saving field {field.Name} on {comp.GetType().Name} ({go.name}): {e.Message}");
                        }
                    }
                }

                // Only add component data if it has savable data (either jsonValue for built-in or fields for custom)
                if (!string.IsNullOrEmpty(compData.jsonValue) || compData.fields.Count > 0)
                {
                    goData.components.Add(compData);
                }
            }
            saveData.gameObjects.Add(goData);
        }

        try
        {
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"UniversalSaveManager: Data saved to: {SavePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"UniversalSaveManager: Failed to write save file to {SavePath}: {e.Message}");
        }

        AssetDatabase.Refresh();
    }

    [ContextMenu("Load All GameObjects")]
    public void LoadAllGameObjects()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning($"UniversalSaveManager: Save file not found at {SavePath}. Nothing to load.");
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            UniversalSaveData loadedData = JsonUtility.FromJson<UniversalSaveData>(json);

            foreach (GameObjectSaveData goData in loadedData.gameObjects)
            {
                GameObject go = GameObject.Find(goData.name);
                if (go != null)
                {
                    go.transform.position = goData.position;
                    go.transform.rotation = goData.rotation;
                    go.transform.localScale = goData.localScale;

                    foreach (ComponentSaveData compData in goData.components)
                    {
                        System.Type compType = System.Type.GetType(compData.typeName);
                        if (compType != null)
                        {
                            Component comp = go.GetComponent(compType);
                            if (comp != null)
                            {
                                // Handle built-in Unity components using jsonValue
                                if (!string.IsNullOrEmpty(compData.jsonValue))
                                {
                                    if (comp is BoxCollider boxCollider)
                                    {
                                        BoxColliderData data = JsonUtility.FromJson<BoxColliderData>(compData.jsonValue);
                                        boxCollider.enabled = data.enabled;
                                        boxCollider.center = data.center;
                                        boxCollider.size = data.size;
                                        boxCollider.isTrigger = data.isTrigger;
                                    }
                                    else if (comp is MeshFilter meshFilter)
                                    {
                                        MeshFilterData data = JsonUtility.FromJson<MeshFilterData>(compData.jsonValue);
                                        // Loading mesh by name is complex and requires a resource loading system.
                                        // For now, we just log the name.
                                        Debug.Log($"MeshFilter on {go.name} should have mesh: {data.meshName}");
                                    }
                                    else if (comp is MeshRenderer meshRenderer)
                                    {
                                        MeshRendererData data = JsonUtility.FromJson<MeshRendererData>(compData.jsonValue);
                                        meshRenderer.enabled = data.enabled;
                                        meshRenderer.receiveShadows = data.receiveShadows;
                                        meshRenderer.shadowCastingMode = data.castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                                        // Loading materials by name is also complex.
                                        Debug.Log($"MeshRenderer on {go.name} should have materials: {string.Join(", ", data.materialNames)}");
                                    }
                                    // Add more built-in component loading here as needed
                                }
                                // Handle custom MonoBehaviour scripts using fields list
                                else if (compData.fields.Count > 0)
                                {
                                    foreach (FieldSaveData fieldData in compData.fields)
                                    {
                                        try
                                        {
                                            FieldInfo field = compType.GetField(fieldData.fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                            if (field != null)
                                            {
                                                object value = JsonUtility.FromJson(fieldData.jsonValue, field.FieldType);
                                                field.SetValue(comp, value);
                                            }
                                            else
                                            {
                                                Debug.LogWarning($"Field {fieldData.fieldName} not found on component {comp.GetType().Name} ({go.name}).");
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.LogError($"Error loading field {fieldData.fieldName} on {comp.GetType().Name} ({go.name}): {e.Message}");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"Component of type {compType.Name} not found on GameObject {go.name} during load.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Type {compData.typeName} not found during component deserialization for {go.name}.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"GameObject with name {goData.name} not found in scene during load.");
                }
            }
            Debug.Log($"UniversalSaveManager: Data loaded from: {SavePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"UniversalSaveManager: Failed to read save file from {SavePath}: {e.Message}");
        }
    }
}



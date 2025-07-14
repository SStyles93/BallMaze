using UnityEngine;
using System.Collections.Generic;

// Base class for all Unity component data
[System.Serializable]
public class BaseUnityComponentData
{
    public bool enabled;
}

// BoxCollider data
[System.Serializable]
public class BoxColliderData : BaseUnityComponentData
{
    public Vector3 center;
    public Vector3 size;
    public bool isTrigger;
}

// MeshFilter data
[System.Serializable]
public class MeshFilterData : BaseUnityComponentData
{
    // Saving mesh is complex. We'll save the mesh name or path if it's an asset.
    // For now, we'll just note its presence or a simple identifier.
    public string meshName; // Or asset path/ID
}

// MeshRenderer data
[System.Serializable]
public class MeshRendererData : BaseUnityComponentData
{
    // Saving materials is complex. We'll save material names or paths/IDs.
    public List<string> materialNames = new List<string>(); // Or asset paths/IDs
    public bool receiveShadows;
    public bool castShadows;
}

// Add more as needed, e.g., RigidbodyData, LightData, CameraData, etc.



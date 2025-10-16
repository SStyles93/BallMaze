//using UnityEngine;

//[RequireComponent (typeof(BoxCollider))]
//public class TriggerSaveZone : MonoBehaviour
//{
//    [Tooltip("The tag of the GameObject that can trigger the save.\nLeave empty to trigger on any GameObject.")]
//    public string triggerTag = "";

//    private void Awake()
//    {
//        GetComponent<BoxCollider>().isTrigger = true;
//    }

//    void OnTriggerEnter(Collider other)
//    {
//        if (string.IsNullOrEmpty(triggerTag) || other.CompareTag(triggerTag))
//        {
//            Debug.Log($"TriggerSaveZone: {other.name} entered, triggering save.");
//            SaveManager.TriggerSave();
//        }
//    }

//    private void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.S))
//        {
//            SaveManager.TriggerSave();
//        }
//        if (Input.GetKeyDown(KeyCode.L))
//        {
//            SaveManager.TriggerLoad();
//        }
//    }

//    // Optional: Draw a gizmo to visualize the trigger zone in the editor
//    void OnDrawGizmos()
//    {
//        Gizmos.color = new Color(0, 1, 0, 0.3f); // Green, semi-transparent
//        Vector3 position, scale;
//        position = transform.position + GetComponent<BoxCollider>().center;
//        scale = GetComponent<BoxCollider>().size;
//        Gizmos.DrawCube(position, scale); // Draw a cube representing the trigger
//    }
//}



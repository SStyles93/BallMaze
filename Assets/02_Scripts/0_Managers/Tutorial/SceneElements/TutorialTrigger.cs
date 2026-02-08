using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class TutorialTrigger : MonoBehaviour
{
    public int triggerIndex; // Set this per trigger in the Inspector
    public GameTutorialManager manager;

    private void Start()
    {
        manager.RegisterTrigger(triggerIndex);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Optional: filter by tag
        if (other.CompareTag("Player"))
        {
            manager.TriggerActivated(triggerIndex);
        }
    }
}

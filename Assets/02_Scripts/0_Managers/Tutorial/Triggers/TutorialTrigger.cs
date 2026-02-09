using MyBox;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public int triggerIndex;
    public bool skipStep = false;
    [ConditionalField("skipStep", true)]
    public string tutorialId; // Set this per trigger in the Inspector
    public GameTutorialManager manager;

    private void Start()
    {
        if (skipStep)
            manager.RegisterTrigger(triggerIndex, () => TutorialManager.Instance.CompleteCurrentStep());
        else
            manager.RegisterTrigger(triggerIndex, () => TutorialManager.Instance.StartTutorial(tutorialId));

    }

    private void OnTriggerEnter(Collider other)
    {
        // Optional: filter by tag
        if (other.CompareTag("Player") || 
            other.CompareTag("Ufo") || 
            other.CompareTag("Rocket"))
        {
            manager.TriggerActivated(triggerIndex);
        }
    }
}

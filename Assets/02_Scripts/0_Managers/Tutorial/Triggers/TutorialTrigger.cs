using MyBox;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class TutorialTrigger : MonoBehaviour
{
    public int triggerIndex;
    public bool skipStep = false;
    [ConditionalField("skipStep", true)]
    public int tutorialIndex; // Set this per trigger in the Inspector
    public GameTutorialManager manager;

    private void Start()
    {
        if (skipStep)
            manager.RegisterTrigger(triggerIndex, () => TutorialManager.Instance.CompleteCurrentStep());
        else
            manager.RegisterTrigger(triggerIndex, () => TutorialManager.Instance.StartTutorial(tutorialIndex));

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

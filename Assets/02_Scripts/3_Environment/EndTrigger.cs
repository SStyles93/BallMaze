using UnityEngine;
using UnityEngine.SceneManagement;
public class EndTrigger : MonoBehaviour
{
    bool wasLevelProcessed = false;

    private void Start()
    {
        wasLevelProcessed = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<Rigidbody>().isKinematic = true;

            if (!wasLevelProcessed)
            {
                LevelManager.Instance.ProcessLevelData();
                wasLevelProcessed = true;
            }

            SavingManager.Instance.SaveSession();

            SceneController.Instance.NewTransition()
                .Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.EndPannel)
                .Perform();
        }
    }
}

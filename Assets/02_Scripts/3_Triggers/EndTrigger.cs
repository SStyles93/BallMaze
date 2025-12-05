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

            LevelManager manager = LevelManager.Instance;
            manager.SetLifeLeftOnLevel(LifeManager.Instance.CurrentLife);
            
            // TODO: CREATE TIME SYSTEM TO CORRECTLY INJECT HERE
            manager.SetTimeValueOnLevel(30);

            if (!wasLevelProcessed)
            {
                manager.ProcessLevelData();
                wasLevelProcessed = true;
            }


            // --- Time Def ---
            TimeManager.IsTimeUpdated = false;
            FindFirstObjectByType<TimeManager>().SaveTimeForLevel();

            SavingManager.Instance.SaveSession();

            SceneController.Instance.NewTransition()
                .Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.EndPannel)
                .Perform();
        }
    }
}

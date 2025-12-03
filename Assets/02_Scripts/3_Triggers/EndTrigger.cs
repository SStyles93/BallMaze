using UnityEngine;
using UnityEngine.SceneManagement;
public class EndTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<Rigidbody>().isKinematic = true;

            LevelManager manager = LevelManager.Instance;
            manager.SetLifeLeftOnLevel(LifeManager.Instance.CurrentLife);
            
            // TODO: CREATE TIME SYSTEM TO CORRECTLY INJECT HERE
            manager.SetTimeValueOnLevel(0);
            manager.ProcessLevelData();


            // --- Time Def ---
            TimeDefinitionSaver.IsTimeUpdated = false;
            FindFirstObjectByType<TimeDefinitionSaver>().SaveTimeForLevel();

            SavingManager.Instance.SaveSession();

            SceneController.Instance.NewTransition()
                .Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.EndPannel)
                .Perform();
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
public class EndTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<Rigidbody>().isKinematic = true;

            
            LevelManager.Instance.SetLevelData(LifeManager.Instance.CurrentLife, 30.0f);

            SavingManager.Instance.SaveSession();

            SceneController.Instance.NewTransition()
                .Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.EndPannel)
                .Perform();
        }
    }
}

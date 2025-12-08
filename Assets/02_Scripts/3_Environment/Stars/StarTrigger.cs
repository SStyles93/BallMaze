using UnityEngine;

public class StarTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            LevelManager.Instance?.IncreaseNumberOfStars();
            VibrationManager.Instance.MultiPop(3);

            gameObject.SetActive(false);
        }
    }
}

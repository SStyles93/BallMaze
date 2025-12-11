using UnityEngine;

public class StarTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            LevelManager.Instance?.IncreaseStarCount();
            
            VibrationManager.Instance?.MultiPop(3);

            AudioManager.Instance?.PlayStarSound();

            gameObject.SetActive(false);
        }
    }
}

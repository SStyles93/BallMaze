using UnityEngine;

public class StarTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameObject colObj = other.gameObject;
        if (colObj.CompareTag("Player") || 
            colObj.CompareTag("Ufo") || 
            colObj.CompareTag("Rocket"))
        {
            LevelManager.Instance?.IncreaseStarCount();
            
            VibrationManager.Instance?.MultiPop(3);

            AudioManager.Instance?.PlayStarSound();

            gameObject.SetActive(false);
        }
    }
}

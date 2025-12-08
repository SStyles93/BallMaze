using UnityEngine;

public class StarTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            LevelManager.Instance?.IncreaseNumberOfStars();

            gameObject.SetActive(false);
        }
    }
}

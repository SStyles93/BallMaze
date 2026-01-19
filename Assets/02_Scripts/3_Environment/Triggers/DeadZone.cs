using System.Collections;
using UnityEngine;

public class DeadZone : MonoBehaviour
{
    [SerializeField] private GameObject continuePannel;

    private Rigidbody playerRigidbody;
    private PlayerMovement playerMovement;

    private void ReplacePlayer()
    {
        // Block, respawn and Unblock player
        playerRigidbody.isKinematic = true;
        playerRigidbody.gameObject.transform.position = playerMovement.CurrentPlatform.position;
        playerRigidbody.isKinematic = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (GameStateManager.Instance?.CurrentGameState != GameState.Playing) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if(playerRigidbody == null) playerRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            if(playerMovement == null) playerMovement = collision.gameObject.GetComponent<PlayerMovement>();

            LifeManager.Instance.RemoveLife();

            if (LifeManager.Instance.CurrentLife > 0)
            {
                // Block, respawn and Unblock player
                ReplacePlayer();
            }
            else
            {
                GameStateManager.Instance.SetState(GameState.WaitingForContinue);
                ReplacePlayer();
                continuePannel.SetActive(true);
            }
        }
    }
}

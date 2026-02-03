using System.Collections;
using UnityEngine;

public class DeadZone : MonoBehaviour
{
    [SerializeField] private GameObject continuePannel;

    private Rigidbody playerRigidbody;
    private PlayerMovement playerMovement;

    private void OnCollisionEnter(Collision collision)
    {
        if (GameStateManager.Instance?.CurrentGameState != GameState.Playing) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if(playerRigidbody == null) playerRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            if(playerMovement == null) playerMovement = collision.gameObject.GetComponent<PlayerMovement>();

            if (!CoreManager.Instance.isDebugPlay)
                LifeManager.Instance.RemoveLife();

            if (playerMovement.State == PlayerState.IsDying) return;
            playerMovement.SetState(PlayerState.IsDying);

            if (LifeManager.Instance.CurrentLife > 0)
            {
                // Block, respawn and Unblock player
                playerMovement.ReplacePlayer();
            }
            else
            {
                GameStateManager.Instance.SetState(GameState.WaitingForContinue);
                continuePannel.SetActive(true);
                playerMovement.ReplacePlayer();
            }
        }
    }
}

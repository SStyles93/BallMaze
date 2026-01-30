using UnityEngine;

public class PiquesDeadZone : MonoBehaviour
{
    [SerializeField] private GameObject continuePannel;

    private Rigidbody playerRigidbody;
    private PlayerMovement playerMovement;
    private PlayerCamera playerCamera;

    private void Awake()
    {
        continuePannel = FindFirstObjectByType<ContinuePannelManager>().gameObject;
    }

    private void ReplacePlayer()
    {
        // Block, respawn and Unblock player
        playerRigidbody.isKinematic = true;
        playerRigidbody.gameObject.transform.position = playerMovement.LastSafePlatform.position;
        playerRigidbody.isKinematic = false;
        playerCamera.SetCameraFollow(true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (GameStateManager.Instance?.CurrentGameState != GameState.Playing) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (playerRigidbody == null) playerRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            if (playerMovement == null) playerMovement = collision.gameObject.GetComponent<PlayerMovement>();
            if(playerCamera == null) playerCamera = collision.gameObject.GetComponent<PlayerCamera>();

            LifeManager.Instance.RemoveLife();

            if (LifeManager.Instance.CurrentLife > 0)
            {
                // Block, respawn and Unblock player
                ReplacePlayer();
            }
            else
            {
                playerCamera.SetCameraFollow(false);
                playerRigidbody.AddForce(new Vector3(Random.Range(0, 1), 10, Random.Range(0, 1)), ForceMode.Impulse);


                GameStateManager.Instance.SetState(GameState.WaitingForContinue);
                ReplacePlayer();
                continuePannel.SetActive(true);
            }
        }
    }
}

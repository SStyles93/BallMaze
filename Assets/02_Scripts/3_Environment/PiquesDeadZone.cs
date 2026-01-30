using System.Collections;
using UnityEngine;

public class PiquesDeadZone : MonoBehaviour
{
    [SerializeField] private GameObject continuePannel;
    [SerializeField] private float deathDelay = 2.0f;
    [SerializeField] private float projectionForce = 15.0f;
    [SerializeField] private float radialForce = 5.0f;

    private Rigidbody playerRigidbody;
    private PlayerMovement playerMovement;
    private PlayerCamera playerCamera;

    private void Awake()
    {
        continuePannel = FindFirstObjectByType<ContinuePannelManager>(FindObjectsInactive.Include).gameObject;
    }

    private void OnTriggerEnter(Collider collision)
    {
        GameStateManager gamestateManager = GameStateManager.Instance;
        if (gamestateManager != null)
            if (GameStateManager.Instance?.CurrentGameState != GameState.Playing) return;


        Debug.Log($"Piques Trigged by {collision.gameObject.name}");
        if (playerRigidbody == null) playerRigidbody = collision.gameObject.GetComponent<Rigidbody>();
        if (playerMovement == null) playerMovement = collision.gameObject.GetComponent<PlayerMovement>();
        if (playerCamera == null) playerCamera = collision.gameObject.GetComponent<PlayerCamera>();
        if (continuePannel == null) continuePannel = FindFirstObjectByType<ContinuePannelManager>(FindObjectsInactive.Include).gameObject;
        
        //Avoid double collision
        if (playerMovement.State == PlayerMovement.PlayerState.IsDying) return;
        playerMovement.State = PlayerMovement.PlayerState.IsDying;

        if (!CoreManager.Instance.isDebugPlay)
            LifeManager.Instance?.RemoveLife();

        Vector3 projectionDir = (collision.transform.position - transform.position).normalized;
        playerRigidbody.AddForce(new Vector3(projectionDir.x * radialForce, projectionForce, projectionDir.z * radialForce), ForceMode.Impulse);

        if (LifeManager.Instance.CurrentLife > 0)
        {
            StartCoroutine(DeathSequenceAfterDelay(deathDelay));
        }
        else
        {
            StartCoroutine(DeathSequenceAfterDelay(deathDelay, true));
        }
    }

    private IEnumerator DeathSequenceAfterDelay(float delay, bool isLastLife = false)
    {
        // Let physics act
        yield return new WaitForSeconds(delay);

        if (isLastLife)
        {
            GameStateManager.Instance.SetState(GameState.WaitingForContinue);
            continuePannel.SetActive(true);
        }
        playerMovement.ReplacePlayer();
    }
}

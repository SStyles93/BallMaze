using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeadZone : MonoBehaviour
{
    [SerializeField] private GameObject continuePannel;

    private Rigidbody playerRigidbody;
    private PlayerMovement playerMovement;

    private bool mustResumeGame = false;

    private void OnEnable()
    {
        LifeManager.Instance.OnLifeRegained += ResumeGame;
    }

    private void OnDisable()
    {
        LifeManager.Instance.OnLifeRegained -= ResumeGame;
    }

    private void Update()
    {
        if (!mustResumeGame) return;
        else
        {
            ReplacePlayer();
            mustResumeGame = false;
        }
    }

    private void ResumeGame()
    {
        mustResumeGame = true;
    }

    private void ReplacePlayer()
    {
        // Block, respawn and Unblock player
        playerRigidbody.isKinematic = true;
        playerRigidbody.transform.position = playerMovement.CurrentPlatform.position;
        playerRigidbody.isKinematic = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if(playerRigidbody == null) playerRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            if(playerMovement == null) playerMovement = collision.gameObject.GetComponent<PlayerMovement>();

            LifeManager.Instance.RemoveLife();

            if (LifeManager.Instance.CurrentLife == 0)
            {
                // Block Player
                playerRigidbody.isKinematic = true;

                continuePannel.SetActive(true);
            }
            else
            {
                // Block, respawn and Unblock player
                ReplacePlayer();
            }
        }
    }
}

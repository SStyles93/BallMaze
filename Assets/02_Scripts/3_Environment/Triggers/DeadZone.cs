using System;
using UnityEngine;

public class DeadZone : MonoBehaviour
{
    bool wasLevelProcessed = false;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            PlayerMovement pMov = collision.gameObject.GetComponent<PlayerMovement>();

            LifeManager.Instance.RemoveLife();

            if (LifeManager.Instance.CurrentLife == 0)
            {
                // Block Player
                rb.isKinematic = true;

                if (!wasLevelProcessed)
                {
                    LevelManager.Instance.MarkLevelAsFailed();
                    wasLevelProcessed = true;
                }
            }
            else
            {
                // Block, respawn and Unblock player
                rb.isKinematic = true;
                collision.gameObject.transform.position = pMov.CurrentPlatform.position;
                rb.isKinematic = false;
            }
        }
    }
}

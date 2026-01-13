using System;
using UnityEngine;

public class DeadZone : MonoBehaviour
{
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;

    bool wasLevelProcessed = false;

    private void Start()
    {
        spawnPosition = GameObject.FindGameObjectWithTag("Respawn").GetComponent<PlayerSpawner>().SpawnPosition;
    }

    private void Update()
    {
        if (spawnPosition == Vector3.zero)
            spawnPosition = GameObject.FindGameObjectWithTag("Respawn").GetComponent<PlayerSpawner>().SpawnPosition;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();

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
                //Block, respawn and Unblock player
                rb.isKinematic = true;
                collision.gameObject.transform.position = spawnPosition;
                rb.isKinematic = false;
            }
        }
    }
}

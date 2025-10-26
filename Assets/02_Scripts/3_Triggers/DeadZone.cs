using System;
using UnityEngine;

public class DeadZone : MonoBehaviour
{
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;
    [SerializeField] private LifePannel lifePannel;

    public int LifeLeft = 3;

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

                // Remove Level Data from saving
                LevelManager.Instance.RemoveCurrentLevelData();

                // Save Session
                SavingManager.Instance.SaveSession();

                // Open EndPannel
                SceneController.Instance.NewTransition()
                    .Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.EndPannel)
                    .Perform();
            }
            else
            {
                rb.isKinematic = true; // Block player
                collision.gameObject.transform.position = spawnPosition; // Replace player
                rb.isKinematic = false; // Unblock player
            }
        }
    }
}

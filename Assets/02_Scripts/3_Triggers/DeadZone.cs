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
        if(spawnPosition == Vector3.zero)
            spawnPosition = GameObject.FindGameObjectWithTag("Respawn").GetComponent<PlayerSpawner>().SpawnPosition;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            collision.gameObject.transform.position = spawnPosition;
            collision.gameObject.GetComponent<Rigidbody>().isKinematic = false;

            LifeManager.Instance.RemoveLife();

            if(LifeManager.Instance.CurrentLife == 0)
            {
                LevelManager.Instance.RemoveCurrentLevelData();

                SavingManager.Instance.SaveSession();

                SceneController.Instance.NewTransition()
                    .Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.EndPannel)
                    .Perform();
            }
        }
    }
}

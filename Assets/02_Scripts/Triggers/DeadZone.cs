using UnityEngine;

public class DeadZone : MonoBehaviour
{
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;

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
        }
    }

}

using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private float offset = 1.0f;
    private Vector3 spawnPosition;

    public Vector3 SpawnPosition => spawnPosition;

    void Start()
    {
        spawnPosition = this.transform.position + Vector3.up * offset;
        Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(spawnPosition, 1.0f);
    }
}

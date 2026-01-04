using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private float offset = 1.0f;
    public Vector3 SpawnPosition => spawnPosition;
    private Vector3 spawnPosition;

    private void OnEnable()
    {
        PhysicalMazeGenerator.OnGenerationFinished += InstantiatePlayer;
    }
    private void OnDisable()
    {
        PhysicalMazeGenerator.OnGenerationFinished -= InstantiatePlayer;
    }

    private void InstantiatePlayer()
    {
        spawnPosition = this.transform.position + Vector3.up * offset;
        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    }
    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.green;
        //Gizmos.DrawSphere(spawnPosition, 1.0f);
    }
}

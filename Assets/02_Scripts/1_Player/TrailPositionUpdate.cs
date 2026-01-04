using UnityEngine;

public class TrailPositionUpdate : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float feetOffset = 0f;
    void LateUpdate()
    {
        if (player == null) return;

        Vector3 pos = player.position;
        pos.y += feetOffset;

        transform.position = pos;
        transform.rotation = Quaternion.identity;
    }
}

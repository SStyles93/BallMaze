using UnityEngine;
using UnityEngine.Rendering;

public class TrailPositionUpdate : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float feetOffset = -0.5f;
    [SerializeField] private GameObject distoritionTrail;


    private void Start()
    {
#if UNITY_ANDROID
        distoritionTrail.SetActive(false);
#endif
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 pos = player.position;
        pos.y += feetOffset;

        transform.position = pos;
        //transform.rotation = Quaternion.identity;
    }
}


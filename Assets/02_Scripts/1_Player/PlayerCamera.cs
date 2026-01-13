using Unity.Cinemachine;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private CinemachineCamera cinemachineCam;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        cinemachineCam = GameObject.FindFirstObjectByType<CinemachineCamera>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Start()
    {
        cinemachineCam.Follow = transform;
    }

    private void Update()
    {
        bool isFalling = transform.position.y < playerMovement.FallThreshold;
        SetCameraFollow(isFalling);
    }

    public void SetCameraFollow(bool isFalling)
    {
        cinemachineCam.Follow = isFalling ? null : transform;
    }
}

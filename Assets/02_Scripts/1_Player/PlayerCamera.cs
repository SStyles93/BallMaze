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
        bool isFalling = transform.position.y < playerMovement.FallThreashold;
        SetIsFalling(isFalling);
    }

    public void SetIsFalling(bool isFalling)
    {
        cinemachineCam.Follow = isFalling ? null : transform;
    }
}

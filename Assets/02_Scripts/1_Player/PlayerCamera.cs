using MyBox;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private CinemachineCamera cinemachineCam;
    private CameraClamp camClamp;
    private PlayerMovement playerMovement;

    [SerializeField] private Vector2 cameraGridLimitsPercentage = new Vector2(0.25f, 0.75f);
    [SerializeField] private float softZone = 1.0f;
    [SerializeField] private Vector2 cameraLimitsX = new Vector2(-100, 100);

    private void Awake()
    {
        cinemachineCam = GameObject.FindFirstObjectByType<CinemachineCamera>();
        if (cinemachineCam != null)
            camClamp = cinemachineCam.GetComponent<CameraClamp>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Start()
    {
        if(LevelManager.Instance != null)
        {
            int width = LevelManager.Instance.CurrentGrid.GetLength(0);

            PhysicalMazeGenerator physicalMazeGeneratorRef = GameObject.FindFirstObjectByType<PhysicalMazeGenerator>();
            if (physicalMazeGeneratorRef != null)
            {
                cameraLimitsX.x = (width-1) * physicalMazeGeneratorRef.cellSize * cameraGridLimitsPercentage.x;
                cameraLimitsX.y = (width-1) * physicalMazeGeneratorRef.cellSize * cameraGridLimitsPercentage.y;
            }

            if(camClamp != null)
            {
                camClamp.SetXLimits(cameraLimitsX);
                camClamp.SetSoftZone(softZone);
            }
        }

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
using Unity.Cinemachine;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private static CinemachineCamera cinemachineCam;
    private CinemachineBasicMultiChannelPerlin multiChannelPerlin;
    private CameraClamp camClamp;

    [SerializeField] private Vector2 cameraGridLimitsPercentage = new Vector2(0.25f, 0.75f);
    [SerializeField] private float softZone = 1.0f;
    [SerializeField] private Vector2 cameraLimitsX = new Vector2(-100, 100);

    [SerializeField] private static float shakeTimer = 0.1f;
    [SerializeField] private float shakeAmplitude = 0.75f;

    private static float currentShakeTimer = 0;

    private void Awake()
    {
        cinemachineCam = GameObject.FindFirstObjectByType<CinemachineCamera>();
        if (cinemachineCam != null)
        {
            camClamp = cinemachineCam.GetComponent<CameraClamp>();
            multiChannelPerlin = cinemachineCam.GetComponent<CinemachineBasicMultiChannelPerlin>();
        }
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
    }

    private void Update()
    {
        if(currentShakeTimer > 0)
        {
            currentShakeTimer -= Time.deltaTime;
            multiChannelPerlin.AmplitudeGain = shakeAmplitude;
        }
        else
        {
            multiChannelPerlin.AmplitudeGain = 0;
        }
    }

    public static void SetCameraFollow(GameObject objectToFollow)
    {
        cinemachineCam.Follow = objectToFollow.transform;
    }

    public static void Shake(string SurfaceTypeNotUsedYet)
    {
        currentShakeTimer = shakeTimer;
    }
}
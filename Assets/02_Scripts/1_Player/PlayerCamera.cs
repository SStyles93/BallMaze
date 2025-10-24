using Unity.Cinemachine;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float cameraYAxisLimit = -5;

    private CinemachineCamera cinemachineCam;

    private void Awake()
    {
        cinemachineCam = GameObject.FindFirstObjectByType<CinemachineCamera>();
    }

    void Start()
    {
        cinemachineCam.Follow = transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y < cameraYAxisLimit)
        {
            cinemachineCam.Follow = null;
        }
        else
        {
            cinemachineCam.Follow = transform;
        }
    }
}

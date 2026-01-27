using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlatformMovement : MonoBehaviour
{
    [SerializeField] private GroundType groundType = GroundType.MovingPlatformH;
    [SerializeField] private AnimationCurve movementCurve;
    [SerializeField] private float cycleDuration = 4f;
    [SerializeField] private float movementAmplitude = 3f;
    [SerializeField] private float pauseDuration = 0.25f;
    [SerializeField] private float slopeThreshold = 0.05f;

    private Rigidbody rb;
    private Vector3 startPosition;
    private float timer;
    private float pauseTimer = 0f;
    private bool isPaused = false;
    

    public float MovementAmplitude { get => movementAmplitude; set => movementAmplitude = value; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Start()
    {
        startPosition = rb.position;
        switch (groundType)
        {
            case GroundType.MovingPlatformH:
                startPosition.x -= movementAmplitude;
                break;

            case GroundType.MovingPlatformV:
                startPosition.z -= movementAmplitude;
                break;
        }
        rb.position = startPosition;
    }

    private void FixedUpdate()
    {
        if (isPaused)
        {
            pauseTimer -= Time.fixedDeltaTime;
            if (pauseTimer <= 0f)
            {
                isPaused = false;
            }
            else
            {
                return; // Skip movement while paused
            }
        }

        timer += Time.fixedDeltaTime;

        float t = (timer % cycleDuration) / cycleDuration;
        float normalizedOffset = Mathf.Clamp(movementCurve.Evaluate(t), 0f, 1f);

        // Calculate slope (approximate derivative)
        float deltaT = 0.001f; // Small step for derivative
        float slope = (movementCurve.Evaluate(Mathf.Clamp01(t + deltaT)) - normalizedOffset) / deltaT;

        // Pause when slope is near zero (platform is "stopped")
        if (Mathf.Abs(slope) < slopeThreshold)
        {
            isPaused = true;
            pauseTimer = pauseDuration;
        }



        Vector3 targetPosition = startPosition;

        switch (groundType)
        {
            case GroundType.MovingPlatformH:
                targetPosition.x += normalizedOffset * movementAmplitude * 2;
                break;

            case GroundType.MovingPlatformV:
                targetPosition.z += normalizedOffset * movementAmplitude * 2;
                break;
        }

        rb.MovePosition(targetPosition);
    }
}

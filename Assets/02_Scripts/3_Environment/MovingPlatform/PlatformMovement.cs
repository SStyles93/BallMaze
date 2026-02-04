using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlatformMovement : MonoBehaviour
{
    [SerializeField] private GroundType groundType = GroundType.MovingPlatformH;
    [SerializeField] private AnimationCurve movementCurve; // 0 = start, 1 = max
    [SerializeField] private float cycleDuration = 4f;
    [SerializeField] private float movementAmplitude = 3f;
    [SerializeField] private float pauseDuration = 0.25f;

    private Rigidbody rb;
    private Vector3 startPosition;
    private float timer;
    private float pauseTimer = 0f;
    private bool isPaused = false;

    public float MovementAmplitude { get => movementAmplitude; set => movementAmplitude = value; }

    public event Action<bool> OnPlatformActive;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Start()
    {
        startPosition = rb.position;

        // Start at the beginning of movement
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
                OnPlatformActive?.Invoke(true);
            }
            else
                return; // Skip movement while paused
        }

        timer += Time.fixedDeltaTime;
        float t = (timer % cycleDuration) / cycleDuration;

        // Evaluate the curve
        float normalizedOffset = Mathf.Clamp01(movementCurve.Evaluate(t));

        // Apply position
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

        // Check for pause at keyframes
        foreach (Keyframe key in movementCurve.keys)
        {
            if (Mathf.Abs(t - key.time) < 0.001f) // tiny threshold
            {
                isPaused = true;
                pauseTimer = pauseDuration;
                OnPlatformActive?.Invoke(false);
                break;
            }
        }
    }
}

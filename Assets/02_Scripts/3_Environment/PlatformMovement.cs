using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlatformMovement : MonoBehaviour
{
    [SerializeField] private GroundType groundType = GroundType.MovingPlatformH;
    [SerializeField] private float movementAmplitude = 3.0f;
    [SerializeField] private float movementPeriod = 3.0f;

    private Rigidbody rb;
    private Vector3 startPosition;

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
    }

    private void FixedUpdate()
    {
        float offset = SineWave.SineWaveEffect(
            movementPeriod,
            -movementAmplitude,
            movementAmplitude
        );

        Vector3 targetPosition = startPosition;

        if (groundType == GroundType.MovingPlatformH)
        {
            targetPosition.x += offset;
        }
        else // Vertical (Z)
        {
            targetPosition.z += offset;
        }

        rb.MovePosition(targetPosition);
    }
}

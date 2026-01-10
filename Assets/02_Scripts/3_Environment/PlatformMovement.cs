using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    [SerializeField] private GroundType groundType = GroundType.MovingPlatformH;
    [SerializeField] private float movementValue = 3.0f;
    [SerializeField] private float movementPeriod = 3.0f;

    private Vector3 originalPosition;
    private Vector3 lastPosition;

    public Vector3 PlatformVelocity { get; private set; }
    public float MovementValue { get => movementValue; set => movementValue = value; }

    private void Start()
    {
        originalPosition = transform.position;
        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        float offset = SineWave.SineWaveEffect(movementPeriod,
            -movementValue, movementValue);

        Vector3 newPosition = originalPosition;

        if (groundType == GroundType.MovingPlatformH)
        {
            newPosition.x += offset;
        }
        else // Vertical
        {
            newPosition.z += offset;
        }

        PlatformVelocity = (newPosition - lastPosition) / Time.fixedDeltaTime;

        transform.position = newPosition;
        lastPosition = newPosition;
    }
}

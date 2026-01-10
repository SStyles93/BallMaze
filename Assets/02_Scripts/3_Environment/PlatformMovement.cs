using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    [SerializeField] private GroundType groundType = GroundType.MovingPlatformH;
    [SerializeField] private float movementValue = 3.0f;
    [SerializeField] private float movementPeriod = 3.0f;

    private Vector3 originalPosition;

    public float MovementValue { get => movementValue; set => movementValue = value; }

    private void Start()
    {
        originalPosition = transform.position;
    }

    private void Update()
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

        transform.position = newPosition;
    }
}

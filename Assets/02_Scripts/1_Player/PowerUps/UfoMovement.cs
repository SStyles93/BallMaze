using UnityEngine;

public class UfoMovement : MonoBehaviour
{
    [Header("UFO Movement")]
    [SerializeField] private float ufoMoveSpeed = 8f;
    [SerializeField] private float ufoSmooth = 10f;

    [Header("UFO Rotation")]
    [SerializeField] private float maxTiltAngle = 20f;
    [SerializeField] private float rotationSmooth = 3f;

    private Vector2 movementInput;
    private Rigidbody ufoRigidbody;
    private AudioSource audioSource;


    private void OnEnable()
    {
        PlayerController.OnMovePerformed += SetMovementValue;
    }

    private void OnDisable()
    {
        PlayerController.OnMovePerformed -= SetMovementValue;
        movementInput = Vector2.zero;
    }

    private void Awake()
    {
        if (!ufoRigidbody)
            ufoRigidbody = GetComponent<Rigidbody>();
        ufoRigidbody.isKinematic = false;
        ufoRigidbody.useGravity = false;

        if (!audioSource)
            audioSource = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        if (ufoRigidbody.isKinematic) return;

        // Movement
        Vector3 desiredVelocity = new Vector3(
            movementInput.x,
            0f,
            movementInput.y
        ) * ufoMoveSpeed;

        ufoRigidbody.linearVelocity = Vector3.Lerp(
            ufoRigidbody.linearVelocity,
            desiredVelocity,
            Time.fixedDeltaTime * ufoSmooth
        );

        // Rotation
        Vector3 moveDir = new Vector3(movementInput.x, 0f, movementInput.y);

        Quaternion yaw = Quaternion.LookRotation(moveDir, Vector3.up);
        float pitch = movementInput.magnitude * maxTiltAngle;

        Quaternion tilt = Quaternion.Euler(pitch, yaw.eulerAngles.y, 0f);

        ufoRigidbody.MoveRotation(
            Quaternion.Lerp(
                ufoRigidbody.rotation,
                tilt,
                Time.fixedDeltaTime * rotationSmooth
            )
        );
    }


    // --- INPUT ---

    private void SetMovementValue(Vector2 input)
    {
        movementInput = input;
    }
}

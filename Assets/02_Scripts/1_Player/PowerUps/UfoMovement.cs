using UnityEngine;

public class UfoMovement : MonoBehaviour
{
    [Header("UFO Movement")]
    [SerializeField] private float ufoMoveSpeed = 8f;
    [SerializeField] private float ufoSmooth = 10f;

    [Header("UFO Rotation")]
    [SerializeField] private float maxTiltAngle = 30f;
    [SerializeField] private float tiltSmooth = 3f;
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

        if(!audioSource)
            audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // ROTATION (Yaw) – face movement direction
        if (movementInput.sqrMagnitude > 0.001f)
        {
            Vector3 moveDir = new Vector3(movementInput.x, 0f, movementInput.y);

            Quaternion targetYaw = Quaternion.LookRotation(moveDir, Vector3.up);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetYaw,
                Time.deltaTime * rotationSmooth
            );
        }

        // TILT (Pitch) – only forward
        float pitch = movementInput.magnitude * maxTiltAngle;

        Quaternion targetTilt = Quaternion.Euler(
            pitch,   // forward tilt
            transform.localEulerAngles.y,
            0f
        );

        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            targetTilt,
            Time.deltaTime * tiltSmooth
        );
    }


    private void FixedUpdate()
    {
        // Horizontal movement
        Vector2 targetVelocity = movementInput * ufoMoveSpeed;

        //// Hover height control
        //Vector3 rigidbodyPos = ufoRigidbody.position;
        //rigidbodyPos.y = ufoHoverHeight;
        //ufoRigidbody.position = rigidbodyPos;

        Vector3 desiredVelocity = new Vector3(
            targetVelocity.x,
            0,
            targetVelocity.y
        );

        if (ufoRigidbody.isKinematic == true) return;

        // Smooth velocity change
        ufoRigidbody.linearVelocity = Vector3.Lerp(
            ufoRigidbody.linearVelocity,
            desiredVelocity,
            Time.fixedDeltaTime * ufoSmooth
        );
    }

    // --- INPUT ---

    private void SetMovementValue(Vector2 input)
    {
        movementInput = input;
    }
}

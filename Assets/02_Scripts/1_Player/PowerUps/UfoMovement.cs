using UnityEngine;
using UnityEngine.Audio;

public class UfoMovement : MonoBehaviour
{
    [Header("UFO Movement")]
    [SerializeField] private float ufoHoverHeight = 3.6f;
    [SerializeField] private float ufoMoveSpeed = 8f;
    [SerializeField] private float ufoSmooth = 10f;

    [Header("UFO Rotation")]
    [SerializeField] private float maxTiltAngle = 15f;
    [SerializeField] private float tiltSmooth = 5f;

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
        float pitch = movementInput.y * maxTiltAngle; // forward/back
        float roll = -movementInput.x * maxTiltAngle; // left/right

        Quaternion targetRotation = Quaternion.Euler(
            pitch,
            0f,
            roll
        );

        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * tiltSmooth
        );

    }

    private void FixedUpdate()
    {
        // Horizontal movement
        Vector2 targetVelocity = movementInput * ufoMoveSpeed;

        // Hover height control
        Vector3 rigidbodyPos = ufoRigidbody.position;
        rigidbodyPos.y = ufoHoverHeight;
        ufoRigidbody.position = rigidbodyPos;

        Vector3 desiredVelocity = new Vector3(
            targetVelocity.x,
            0,
            targetVelocity.y
        );

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

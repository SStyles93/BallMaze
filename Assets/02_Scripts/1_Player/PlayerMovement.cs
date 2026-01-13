using PxP.Draw;
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody playerRigidbody;

    [Header("Rolling")]
    [SerializeField] private float torqueStrength = 30f;
    [SerializeField] private float maxAngularVelocity = 25f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float gravityScale = 2f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 1.1f;
    [SerializeField] private float groundDetectionRadius = 0.35f;
    [SerializeField] private LayerMask groundedLayerMask;

    [Header("Fall")]
    [SerializeField] private float fallThreshold = -5f;


    private Vector3 movementInput;
    private bool isGrounded;
    private bool wasJumpPerformed;
    // --- Platform ---
    private bool allowRotation;
    private Transform currentPlatform;

    public event Action<string> OnPlayerLanded;
    public event Action OnPlayerJumped;


    public float FallThreshold { get => fallThreshold; set => fallThreshold = value; }
    public Rigidbody PlayerRigidbody { get => playerRigidbody; set => playerRigidbody = value; }

    private void OnEnable()
    {
        PlayerControler.OnMovePerfromed += SetMovementValue;
        PlayerControler.OnJumpPerformed += Jump;
    }

    private void OnDisable()
    {
        PlayerControler.OnMovePerfromed -= SetMovementValue;
        PlayerControler.OnJumpPerformed -= Jump;
    }

    private void Awake()
    {
        if (!playerRigidbody)
            playerRigidbody = GetComponent<Rigidbody>();

        playerRigidbody.maxAngularVelocity = maxAngularVelocity;
        playerRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        CheckGrounded();

        CheckIfRotationIsAllowed();

        ApplyExtraGravity();

        if (allowRotation) Roll();
    }
    // ---------------- ROLLING ----------------

    private void Roll()
    {
        if (!isGrounded || movementInput.sqrMagnitude < 0.01f)
            return;

        // Torque direction perpendicular to player input
        Vector3 torque = Vector3.Cross(Vector3.up, movementInput.normalized);
        playerRigidbody.AddTorque(torque * torqueStrength, ForceMode.Acceleration);
    }

    // ---------------- JUMP ----------------

    private void Jump()
    {
        if (!isGrounded) return;

        Vector3 iceDirectionHelper = Vector3.zero;
        if (currentPlatform.CompareTag("Ice"))
            iceDirectionHelper = movementInput * 0.5f;

        playerRigidbody.AddForce((Vector3.up + iceDirectionHelper).normalized * jumpForce, ForceMode.Impulse);
        isGrounded = false;
        wasJumpPerformed = true;

        // Calls the JumpSound on the PlayerSound script
        OnPlayerJumped?.Invoke();
        
        // Unset platform reference when jumping
        currentPlatform = null;
    }

    // ---------------- GROUND ----------------

    private void CheckGrounded()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        isGrounded = Physics.SphereCast(ray, groundDetectionRadius, out hit, groundCheckDistance, groundedLayerMask);

        if (isGrounded)
        {
            if (hit.collider != null)
            {
                currentPlatform = hit.collider.transform;
            }

            // Y velocity is generally at -8.smth (if < 0, this part might trigger before falling)
            if (wasJumpPerformed && playerRigidbody.linearVelocity.y < -2f)
            {
                //Debug.Log(playerRigidbody.linearVelocity);
                // Calls the LandedSound on the PlayerSound script
                OnPlayerLanded?.Invoke(hit.collider.tag);

                wasJumpPerformed = false;
            }

            DebugDraw.Capsule(ray, groundDetectionRadius, groundCheckDistance, Color.green);
        }
        else
        {
            DebugDraw.Capsule(ray, groundDetectionRadius, groundCheckDistance, Color.red);
            currentPlatform = null;
        }
    }

    // ---------------- PLATFORM COUNTER-TORQUE ----------------

    private void CheckIfRotationIsAllowed()
    {
        allowRotation = isGrounded && movementInput.sqrMagnitude > 0.01f;

        if (!isGrounded)
            return;

        if (currentPlatform != null && !allowRotation && currentPlatform.CompareTag("MovingPlatform"))
        {
            playerRigidbody.angularVelocity =
            Vector3.Lerp(playerRigidbody.angularVelocity, Vector3.zero, 0.4f);
        }
        
    }

    // ---------------- INPUT ----------------

    private void SetMovementValue(Vector2 input)
    {
        Vector2 mapped = DirectionMapper.MapTo8CardinalPoints(input);
        movementInput = new Vector3(mapped.x, 0, mapped.y);
    }

    // ---------------- GRAVITY ----------------

    private void ApplyExtraGravity()
    {
        if (!isGrounded)
            playerRigidbody.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
    }
}
public class DirectionMapper : MonoBehaviour
{
    /// <summary>
    /// Maps an analog Vector2 input to the nearest of 8 cardinal directions.
    /// </summary>
    /// <param name="analogInput">The raw input vector (e.g., from a joystick axis, range -1 to 1).</param>
    /// <param name="deadzone">Minimum magnitude for input to be considered, typically 0.1f.</param>
    /// <returns>A Vector2 snapped to one of the 8 cardinal directions, or Vector2.zero if within the deadzone.</returns>
    public static Vector2 MapTo8CardinalPoints(Vector2 analogInput, float deadzone = 0.1f)
    {
        float magnitude = analogInput.magnitude;

        // Deadzone check
        if (magnitude < deadzone)
            return Vector2.zero;

        // Normalize only for angle calculation
        Vector2 normalizedInput = analogInput / magnitude;

        // Angle in degrees
        float angle = Mathf.Atan2(normalizedInput.y, normalizedInput.x) * Mathf.Rad2Deg;
        if (angle < 0)
            angle += 360f;

        // Snap angle to nearest 45�
        float snappedAngle = Mathf.Round(angle / 45f) * 45f;
        snappedAngle %= 360f;

        // Direction from snapped angle (UNIT vector)
        Vector2 snappedDirection = new Vector2(
            Mathf.Cos(snappedAngle * Mathf.Deg2Rad),
            Mathf.Sin(snappedAngle * Mathf.Deg2Rad)
        );

        // Reapply original magnitude (ANALOG)
        return snappedDirection * magnitude;
    }
}

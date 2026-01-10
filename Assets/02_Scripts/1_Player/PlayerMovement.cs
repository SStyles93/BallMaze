using PxP.Draw;
using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Reference Components")]
    [SerializeField] Rigidbody playerRigidbody = null;

    [Header("Movement Settings")]
    [SerializeField] private ForceMode movementForceMode = ForceMode.VelocityChange;
    [SerializeField] private float movementForce = 30.0f;
    [SerializeField] private float maxVelocity = 10.0f;
    [SerializeField] private float linearDampingGroundValue = 4.0f;
    [SerializeField] private float linearDampingAirValue = 4.0f;

    [Header("Jump Settings")]
    [SerializeField] private ForceMode jumpForceMode = ForceMode.Impulse;
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float gravityScale = 2.0f;
    [SerializeField] float groundCheckDistance = 1.1f;
    [SerializeField] float groundDetectionRadius = 0.35f;
    [SerializeField] LayerMask groundedLayerMask;

    [Header("Fall Settings")]
    [SerializeField] private float fallThreashold = -5.0f;

    [SerializeField] private bool isGrounded = false;
    [SerializeField] private bool wasJumpPerfromed = false;
    [SerializeField] private Vector3 movementValue;

    private Rigidbody currentPlatformRb;
    private Vector3 currentPlatformVelocity;

    private bool isOnIce = false;

    public float FallThreashold => fallThreashold;

    public float MovementForce { get => movementForce; set => movementForce = value; }
    public Rigidbody Rigidbody { get => playerRigidbody; set => playerRigidbody = value; }

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
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            playerRigidbody = rb;
        }
        else Debug.Log($"No Rigidbody found on {this.gameObject.name}");
    }

    private void FixedUpdate()
    {
        UpdateFallGravity();

        RaycastHit groundHit;
        isGrounded = CheckIfPlayerIsGrounded(out groundHit);

        currentPlatformVelocity = Vector3.zero;

        if (isGrounded)
        {
            if (groundHit.collider.TryGetComponent<PlatformMovement>(out var platform))
            {
                currentPlatformVelocity = platform.PlatformVelocity;
            }
        }

        // linearDamping is canceled with no movement, otherwise ground/air value accordingly
        float linearDamping = 1.0f;
        if (isOnIce)
        {
            linearDamping = 0.05f;
        }
        else if (isGrounded)
        {
            linearDamping = movementValue.magnitude > 0.1f ? 
                1.0f : linearDampingGroundValue;
        }
        else
            linearDamping = linearDampingAirValue;

        playerRigidbody.linearDamping = linearDamping;


        if (isGrounded)
        {
            if(playerRigidbody.linearVelocity.magnitude > maxVelocity) return;

            Vector3 appliedForce = movementValue * MovementForce * Time.deltaTime;
            
            //Check for Ice impact, Reduce input impact on ice
            float controlMultiplier = isOnIce ? 0.25f : 1.0f;
            appliedForce *= controlMultiplier;
            
            // Apply movement with force
            playerRigidbody.AddForce(appliedForce, movementForceMode);

            // Ad Platform velocity
            Vector3 velocity = playerRigidbody.linearVelocity;

            velocity.x += currentPlatformVelocity.x;
            velocity.z += currentPlatformVelocity.z;

            playerRigidbody.linearVelocity = velocity;

            //Debug.Log($"Force Applied {(movementValue * MovementForce * Time.deltaTime).magnitude} " +
            //    $"in {movementValue} direction, " +
            //    $"with {movementForceMode.ToString()}");

            //Debug.Log($"{playerRigidbody.linearVelocity.magnitude}");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        int collisionLayer = collision.collider.gameObject.layer;
        if ((1 << collisionLayer & groundedLayerMask) != 0 && wasJumpPerfromed)
        {
            wasJumpPerfromed = false;
            AudioManager.Instance?.PlayThumpSound();
        }
    }

    /// <summary>
    /// Does what it says with Phisics Raycast
    /// </summary>
    private bool CheckIfPlayerIsGrounded(out RaycastHit hit)
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.SphereCast(ray, groundDetectionRadius, out hit, groundCheckDistance, groundedLayerMask))
        {
#if UNITY_EDITOR
            DebugDraw.Capsule(ray, groundDetectionRadius, groundCheckDistance, Color.green);
#endif
            // 17 is the ICE LAYER
            isOnIce = hit.collider.gameObject.layer == 17;
            return true;
        }
        else
        {
#if UNITY_EDITOR
            DebugDraw.Capsule(ray, groundDetectionRadius, groundCheckDistance, Color.red);
#endif
            return false;
        }
    }

    /// <summary>
    /// Method used to make the player jump
    /// </summary>
    /// <remarks>Player has to be grounded</remarks>
    private void Jump()
    {
        // If the player is in the air we don't want a second jump
        if (!isGrounded) return;
        isGrounded = false;
        playerRigidbody.AddForce(Vector3.up * jumpForce, jumpForceMode);
        //Debug.Log($"Force Applied up with {jumpForce} force, and {movementForceMode.ToString()}");
        VibrationManager.Instance?.Pop();
        AudioManager.Instance?.PlayJumpSound();
        wasJumpPerfromed = true;
    }

    /// <summary>
    /// Used to get the movement direction
    /// </summary>
    /// <param name="movementDirection">Movement direction (Vector2)</param>
    private void SetMovementValue(Vector2 movementDirection)
    {
        Vector2 remappedInput = DirectionMapper.MapTo8CardinalPoints(movementDirection);
        Vector3 axisAlignedInput = new Vector3(remappedInput.x, 0, remappedInput.y);
        movementValue = axisAlignedInput;
        //Debug.Log(movementValue);
    }

    /// <summary>
    /// Updates the gravity for a better fall
    /// </summary>
    private void UpdateFallGravity()
    {
        Vector3 gravity = Physics.gravity.y * gravityScale * Vector3.up;
        playerRigidbody.AddForce(gravity, ForceMode.Acceleration);
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

        // Snap angle to nearest 45°
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

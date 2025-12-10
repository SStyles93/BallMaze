using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Reference Components")]
    [SerializeField] Rigidbody playerRigidbody = null;

    [Header("Movement Settings")]
    [SerializeField] private ForceMode movementForceMode = ForceMode.Force;
    [SerializeField] private float movementForce = 5.0f;

    [Header("Jump Settings")]
    [SerializeField] private ForceMode jumpForceMode = ForceMode.Impulse;
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float gravityScale = 2.0f;
    [SerializeField] float groundCheckDistance = 1.1f;
    [SerializeField] LayerMask groundedLayerMask;

    [SerializeField] private bool isGrounded = false;
    [SerializeField] private Vector3 movementValue;

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

    private void Update()
    {
#if UNITY_EDITOR
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, Color.green);
#endif
    }

    private void FixedUpdate()
    {
        UpdateFallGravity();

        isGrounded = CheckIfPlayerIsGrounded();
        playerRigidbody.linearDamping = isGrounded ? 2.0f : 0.5f;

        if (isGrounded)
        {
            // Apply movement with force
            playerRigidbody.AddForce(movementValue * movementForce * Time.deltaTime, movementForceMode);
            //Debug.Log($"Force Applied in {movementDirection} direction, with {movementForceMode.ToString()}");
        }
    }

    /// <summary>
    /// Does what it says with Phisics Raycast
    /// </summary>
    private bool CheckIfPlayerIsGrounded()
    {
        if (Physics.Raycast(transform.position, Vector3.down, groundCheckDistance))
            return true;
        else
            return false;
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
        // Check if the input magnitude is below the deadzone
        if (analogInput.magnitude < deadzone)
        {
            return Vector2.zero;
        }

        // Calculate the angle in degrees from the input vector (atan2 returns angle in radians)
        // Note: In Unity's 2D system, Atan2(y, x) is standard for angle calculation
        float angle = Mathf.Atan2(analogInput.y, analogInput.x) * Mathf.Rad2Deg;

        // Ensure the angle is positive (0 to 360) instead of (-180 to 180)
        if (angle < 0)
        {
            angle += 360;
        }

        // Snap the angle to the nearest 45-degree increment
        // There are 8 directions, so 360 / 8 = 45 degrees per direction
        float snappedAngle = Mathf.Round(angle / 45f) * 45f;

        // Wrap around 360 degrees to 0 if necessary
        snappedAngle %= 360;

        // Convert the snapped angle back to a directional Vector2
        float horizontalOut = Mathf.Cos(snappedAngle * Mathf.Deg2Rad);
        float verticalOut = Mathf.Sin(snappedAngle * Mathf.Deg2Rad);

        // Round the results to clean integer/near-integer values (-1, 0, 1) for axis-aligned movement, 
        // otherwise they will be floats due to Cos/Sin calculation
        horizontalOut = Mathf.Round(horizontalOut);
        verticalOut = Mathf.Round(verticalOut);

        // Normalize the vector
        Vector2 snappedDirection = new Vector2(horizontalOut, verticalOut).normalized; 

        return snappedDirection;
    }
}

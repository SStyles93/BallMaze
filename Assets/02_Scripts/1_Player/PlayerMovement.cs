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

    private bool isGrounded = false;
    private Vector3 movementValue;

    private void OnEnable()
    {
        PlayerControler.OnMovePerfromed += GetMovementValue;
        PlayerControler.OnJumpPerformed += Jump;
    }

    private void OnDisable()
    {
        PlayerControler.OnMovePerfromed -= GetMovementValue;
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
        if (Physics.Raycast(transform.position, Vector3.down, groundCheckDistance))
            isGrounded = true;
        else
            isGrounded = false;
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, Color.green);
    }

    private void FixedUpdate()
    {
        // Updates the gravity for a better fall
        Vector3 gravity = Physics.gravity.y * gravityScale * Vector3.up;
        playerRigidbody.AddForce(gravity, ForceMode.Acceleration);

        // Apply movement with force
        playerRigidbody.AddForce(movementValue * movementForce * Time.deltaTime, movementForceMode);

        //Debug.Log($"Force Applied in {movementDirection} direction, with {movementForceMode.ToString()}");
    }

    /// <summary>
    /// Used to get the movement direction
    /// </summary>
    /// <param name="movementDirection">Movement direction (Vector2)</param>
    private void GetMovementValue(Vector2 movementDirection)
    {

        if (!isGrounded)
        {
            movementValue = Vector3.zero;
            return;
        }
        // Correct movement for x/z directions
        movementValue = new Vector3(movementDirection.x, 0, movementDirection.y);
    }

    /// <summary>
    /// Method used to make the player jump
    /// </summary>
    /// <remarks>Player has to be grounded</remarks>
    private void Jump()
    {
        if (!isGrounded) return;
        isGrounded = false;
        //playerRigidbody.linearDamping = 1f;
        playerRigidbody.AddForce(Vector3.up * jumpForce, jumpForceMode);
        //Debug.Log($"Force Applied up with {jumpForce} force, and {movementForceMode.ToString()}");
    }
}

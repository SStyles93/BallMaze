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
    [SerializeField] private float gravityScale = 3.0f;
    [SerializeField] LayerMask groundedLayerMask;

    private bool isGrounded = false;

    private void OnEnable()
    {
        PlayerControler.OnMovePerfromed += Move;
        PlayerControler.OnJumpPerformed += Jump;
    }

    private void OnDisable()
    {
        PlayerControler.OnMovePerfromed -= Move;
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
        // Updates the gravity for a better fall
        Vector3 gravity = Physics.gravity.y * gravityScale * Vector3.up;
        playerRigidbody.AddForce(gravity, ForceMode.Acceleration);
    }

    /// <summary>
    /// Used to move the player in the given direction
    /// </summary>
    /// <param name="movementDirection">Movement direction (Vector2)</param>
    private void Move(Vector2 movementDirection)
    {
        if (!isGrounded) return;

        // Correct movement for x/z directions
        Vector3 movement = new Vector3(movementDirection.x, 0, movementDirection.y);
        
        // Apply movement with force
        playerRigidbody.linearDamping = 2;
        playerRigidbody.AddForce(movement * movementForce, movementForceMode);
        
        //Debug.Log($"Force Applied in {movementDirection} direction, with {movementForceMode.ToString()}");
    }

    /// <summary>
    /// Method used to make the player jump
    /// </summary>
    /// <remarks>Player has to be grounded</remarks>
    private void Jump()
    {
        if (!isGrounded) return;
        isGrounded = false;
        playerRigidbody.linearDamping = 1;
        playerRigidbody.AddForce(Vector3.up * jumpForce, jumpForceMode);
        //Debug.Log($"Force Applied up with {jumpForce} force, and {movementForceMode.ToString()}");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(((1 << collision.gameObject.layer) & groundedLayerMask) != 0)
        {
            isGrounded = true;
        }
    }
}

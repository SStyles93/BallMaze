using UnityEngine;

public class DoorAngularPush : MonoBehaviour
{
    [SerializeField] private float pushForce = 10.0f;

    [SerializeField] private Transform parentTransform;
    [SerializeField] private DoorAngularPush otherDoor;
    [SerializeField] FlapingDoorsAnimation doorsAnimation;

    private bool isPlayerOnDoor = false;
    private bool hasCollided = false;

    public bool HasCollided => hasCollided;

    private void OnEnable()
    {
        doorsAnimation.OnPauseEnded += EnableCollision;
    }

    private void OnDisable()
    {
        doorsAnimation.OnPauseEnded -= EnableCollision;
    }

    private void EnableCollision()
    {
        hasCollided = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isPlayerOnDoor = true;
            if (doorsAnimation.State == FlapingDoorsAnimation.DoorState.Opening && !hasCollided && !otherDoor.hasCollided)
            {
                Vector3 parentPos = parentTransform.position;
                Vector3 playerPos = collision.collider.transform.position;

                // Horizontal distance from center
                float distanceFromCenter = Mathf.Abs(playerPos.x - parentPos.x);

                // Max distance where force becomes 0
                float maxEffectiveDistance = parentTransform.localScale.x * 0.5f;

                // Normalize and invert
                float normalized = Mathf.Clamp01(distanceFromCenter / maxEffectiveDistance);

                // Direction (left/right)
                float direction = Mathf.Sign(playerPos.x - parentPos.x);

                // Apply force
                Vector3 force = new Vector3(
                    direction * normalized,
                    direction * normalized,
                    0f
                );

                collision.rigidbody.AddForce(force * pushForce, ForceMode.Impulse);

                hasCollided = true;

                Debug.DrawLine(parentPos, playerPos, Color.Lerp(Color.green, Color.red, normalized),2f);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isPlayerOnDoor = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (isPlayerOnDoor)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}
using UnityEngine;

public class DoorAngularPush : MonoBehaviour
{
    [SerializeField] private float pushForce = 10.0f;
    [SerializeField] float maxEffectiveDistance = 2.0f;

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

                float distanceFromCenter = Mathf.Abs(playerPos.x - parentPos.x);
                float normalized = 1f - Mathf.Clamp01(distanceFromCenter / maxEffectiveDistance);
                float direction = Mathf.Sign(playerPos.x - parentPos.x);

                Vector3 force = new Vector3(
                    direction * pushForce * normalized,
                    pushForce * normalized,
                    0f
                );

                collision.rigidbody.AddForce(force, ForceMode.Impulse);

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

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(parentTransform.position, Vector3.one * maxEffectiveDistance);
    }
}
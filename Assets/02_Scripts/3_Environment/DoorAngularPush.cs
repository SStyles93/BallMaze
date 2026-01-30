using UnityEngine;

public class DoorAngularPush : MonoBehaviour
{
    [Header("Push Settings")]
    [SerializeField] private float maxPushForce = 12f;
    [SerializeField] private AnimationCurve forceFromAngularSpeed;
    [SerializeField] private float maxAngularSpeed = 90f; // deg/sec

    private Quaternion lastRotation;

    private void Awake()
    {
        lastRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        lastRotation = transform.rotation;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!collision.rigidbody) return;

        // Angular delta this frame
        Quaternion delta = transform.rotation * Quaternion.Inverse(lastRotation);
        delta.ToAngleAxis(out float angle, out Vector3 axis);

        float angularSpeed = angle / Time.fixedDeltaTime;

        float normalizedSpeed = Mathf.Clamp01(angularSpeed / maxAngularSpeed);
        float forceMultiplier = forceFromAngularSpeed.Evaluate(normalizedSpeed);

        Vector3 pushDirection = transform.right; // tweak if needed
        pushDirection += Vector3.up * 0.3f;


        collision.rigidbody.AddForce(
            pushDirection * maxPushForce * forceMultiplier,
            ForceMode.Impulse
        );
    }
}

using UnityEngine;

public class FlapingDoorsAnimation : MonoBehaviour
{
    [Header("Door References")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    [Header("Animation Settings")]
    [SerializeField] private AnimationCurve openCurve;
    [SerializeField] private float openDuration = 1f;
    [SerializeField] private float openPauseDuration = 0.5f;
    [SerializeField] private AnimationCurve closeCurve;
    [SerializeField] private float closeDuration = 1f;
    [SerializeField] private float closePauseDuration = 0.5f;
    [Space(10)]
    [SerializeField] private float flapAngle = 90f;

    private enum DoorState
    {
        Opening,
        PausingOpen,
        Closing,
        PausingClosed
    }

    private DoorState state = DoorState.Opening;
    private float stateTimer;

    private Quaternion leftClosedRotation;
    private Quaternion rightClosedRotation;

    private void Start()
    {
        leftClosedRotation = leftDoor.localRotation;
        rightClosedRotation = rightDoor.localRotation;
    }

    private void Update()
    {
        stateTimer += Time.deltaTime;

        switch (state)
        {
            case DoorState.Opening:
                Animate(openCurve, openDuration, DoorState.PausingOpen);
                break;

            case DoorState.Closing:
                Animate(closeCurve, closeDuration, DoorState.PausingClosed);
                break;

            case DoorState.PausingOpen:
                if (stateTimer >= openPauseDuration)
                    NextState();
                break;
            case DoorState.PausingClosed:
                if (stateTimer >= closePauseDuration)
                    NextState();
                break;
        }
    }

    private void Animate(AnimationCurve curve, float duration, DoorState next)
    {
        float t = Mathf.Clamp01(stateTimer / duration);
        float value = curve.Evaluate(t);

        leftDoor.localRotation =
            leftClosedRotation * Quaternion.Euler(0f, 0f, value * flapAngle);

        rightDoor.localRotation =
            rightClosedRotation * Quaternion.Euler(0f, 0f, -value * flapAngle);

        if (t >= 1f)
        {
            state = next;
            stateTimer = 0f;
        }
    }

    private void NextState()
    {
        stateTimer = 0f;

        state = state switch
        {
            DoorState.PausingOpen => DoorState.Closing,
            DoorState.PausingClosed => DoorState.Opening,
            _ => state
        };
    }
}

using System;
using UnityEngine;

public class FlapingDoorsAnimation : MonoBehaviour, ITimedHazard
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

    public event Action OnDoorOpening;

    public enum DoorState
    {
        Opening,
        PausingOpen,
        Closing,
        PausingClosed
    }

    [Space(20)]
    [SerializeField] private DoorState state = DoorState.Opening;
    private float stateTimer;

    private Quaternion leftClosedRotation;
    private Quaternion rightClosedRotation;

    private bool isOpening = false;

    public DoorState State => state;
    public float StateTimer => stateTimer;

    public event Action OnPauseEnded;

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
                if (!isOpening)
                {
                    OnDoorOpening?.Invoke();
                    isOpening = true;
                }
                Animate(openCurve, openDuration, DoorState.PausingOpen);
                break;

            case DoorState.Closing:
                isOpening = false;
                Animate(closeCurve, closeDuration, DoorState.PausingClosed);
                break;

            case DoorState.PausingOpen:
                if (stateTimer >= openPauseDuration)
                    NextState();
                break;
            case DoorState.PausingClosed:
                if (stateTimer >= closePauseDuration)
                {
                    OnPauseEnded?.Invoke();
                    NextState();
                }
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

    // --- TIME SETTINGS ---

    public float CycleDuration =>
    openDuration + openPauseDuration +
    closeDuration + closePauseDuration;

    public void SetState(bool isInverted)
    {
        if (isInverted)
        {
            state = DoorState.Closing;
            stateTimer = CycleDuration / 2;
        }
        else
        {
            state = DoorState.Opening;
        }
    }

    public bool IsSafe() => state == DoorState.PausingClosed;
}

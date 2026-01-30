using UnityEngine;

public class PiquesTile : MonoBehaviour
{
    [Header("Piques References")]
    [SerializeField] private Transform piques;

    [Header("Animation")]
    [SerializeField] private AnimationCurve riseCurve;   // down → up
    [SerializeField] private float riseDuration = 0.6f;
    [SerializeField] private AnimationCurve lowerCurve;  // up → down
    [SerializeField] private float lowerDuration = 0.6f;
    [Space(10)]
    [SerializeField] private float pauseDuration = 0.5f;
    [Space(10)]
    [SerializeField] private float piquesYOffset = 1.5f;

    private enum PiquesState
    {
        Rising,
        PausedUp,
        Lowering,
        PausedDown
    }

    private PiquesState state = PiquesState.Rising;
    private float stateTimer;
    private Vector3 piqueOriginalPosition;

    private void Start()
    {
        piqueOriginalPosition = piques.localPosition;
    }

    private void Update()
    {
        stateTimer += Time.deltaTime;

        switch (state)
        {
            case PiquesState.Rising:
                Animate(riseCurve, riseDuration, PiquesState.PausedUp);
                break;

            case PiquesState.Lowering:
                Animate(lowerCurve, lowerDuration, PiquesState.PausedDown);
                break;

            case PiquesState.PausedUp:
            case PiquesState.PausedDown:
                if (stateTimer >= pauseDuration)
                    NextState();
                break;
        }
    }

    private void Animate(AnimationCurve curve, float duration, PiquesState next)
    {
        float t = Mathf.Clamp01(stateTimer / duration);
        float value = curve.Evaluate(t);

        piques.localPosition =
            piqueOriginalPosition + Vector3.up * piquesYOffset * value;

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
            PiquesState.PausedUp => PiquesState.Lowering,
            PiquesState.PausedDown => PiquesState.Rising,
            _ => state
        };
    }


}

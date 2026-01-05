using UnityEngine;
using DG.Tweening;

public class PlayerVisualEffects : MonoBehaviour
{
    [SerializeField] private float shrinkDuration = 1f;
    [SerializeField] private TrailRenderer m_trailRenderer;


    private PlayerMovement playerMovement;
    private Tween scaleTween;

    private enum ScaleState
    {
        Normal,
        Shrunk
    }

    private ScaleState state = ScaleState.Normal;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        m_trailRenderer = GetComponentInChildren<TrailRenderer>();

        // Create the tween ONCE and reuse it
        scaleTween = transform
            .DOScale(Vector3.zero, shrinkDuration)
            .SetEase(Ease.InOutQuad)
            .SetAutoKill(false)
            .Pause()
            .SetLink(gameObject)
            .OnRewind(EnableTrail);
    }

    private void Update()
    {
        bool shouldShrink = transform.position.y < playerMovement.FallThreashold;

        if (shouldShrink && state == ScaleState.Normal)
        {
            Shrink();
        }
        else if (!shouldShrink && state == ScaleState.Shrunk)
        {
            Grow();
        }
    }

    private void Shrink()
    {
        state = ScaleState.Shrunk;
        scaleTween.PlayForward();
    }

    private void Grow()
    {
        state = ScaleState.Normal;
        scaleTween.PlayBackwards();
    }

    private void EnableTrail()
    {
        m_trailRenderer.enabled = true;
    }
}

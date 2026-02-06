using UnityEngine;
using DG.Tweening;
using System.Linq;

public class PlayerVisualEffects : MonoBehaviour
{
    [Header("Animations")]
    [SerializeField] private float shrinkDuration = 1f;
    [SerializeField] private float blinkDuration = 0.1f;
    [SerializeField] private int blinkCount = 6;
    [SerializeField] private float blinkDelay = 0.5f;
    [SerializeField] private GameObject visualsParent;

    [Header("Trail")]
    [SerializeField] private GameObject m_trail;
    [SerializeField] private float m_trailVelocityThreshold = 1.0f;
    [SerializeField] private Color m_trailColor;
    [SerializeField] private Material[] m_trailMaterials;


    private PlayerMovement playerMovement;
    private Tween scaleTween;
    private Tween blinkTween;
    private Tween trailScaleTween;
    private Renderer[] renderers;
    private TrailRenderer[] trailRenderers;
    private bool m_isTrailActive = true;
    private float startDelay = 0.5f;
    private Vector3 originalScale;
    private float originalTrailWidth;

    private bool shouldShrink = false;
    private bool shouldGrow = false;
    private bool shouldBlink = false;

    private enum AnimationState
    {
        Idle,
        Shrink,
        Shrunk,
        Grow,
        Blink
    }

    private AnimationState state = AnimationState.Idle;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        trailRenderers = m_trail.GetComponentsInChildren<TrailRenderer>();
        originalScale = transform.localScale;
        originalTrailWidth = trailRenderers[0].widthMultiplier;
    }

    private void OnEnable()
    {
        // Player Scale
        scaleTween = transform
            .DOScale(originalScale, shrinkDuration)
            .SetEase(Ease.InOutQuad)
            .SetAutoKill(false)
            .Pause()
            .SetLink(gameObject)
            .OnRewind(OnShrinkComplete)
            .OnComplete(OnGrowComplete);

        trailScaleTween = DOTween.To(
        () => trailRenderers[0].widthMultiplier,
        value =>
        {
            foreach (var tr in trailRenderers)
                tr.widthMultiplier = value;
        },
        originalTrailWidth,
        shrinkDuration)
            .SetEase(Ease.InOutQuad)
            .SetAutoKill(false)
            .Pause()
            .SetLink(gameObject);
    }

    private void OnDisable()
    {
        blinkTween?.Kill();
        trailScaleTween?.Kill();
        scaleTween?.Kill();
    }

    private void Start()
    {
        state = AnimationState.Shrunk;
        shouldGrow = true;

        transform.localScale = Vector3.zero;
        foreach (var tr in trailRenderers)
            tr.widthMultiplier = 0;
    }
    private void Update()
    {
        if (GameStateManager.Instance?.CurrentGameState != GameState.Playing) return;
        if (startDelay > 0.0f)
        {
            startDelay -= Time.deltaTime;
            return;
        }

        EvaluateAnimationState();
    }

    private void FixedUpdate()
    {
        if (playerMovement.PlayerRigidbody.linearVelocity.magnitude < m_trailVelocityThreshold || !m_isTrailActive)
        {
            m_trail.SetActive(false);
        }
        else
        {
            m_trail.SetActive(true);
        }
    }

    public void ForcePlayerShrunk()
    {
        transform.localScale = Vector3.zero;
        state = AnimationState.Shrunk;
        scaleTween.Rewind();
        trailScaleTween.Rewind();
        blinkTween?.Kill();
        shouldGrow = true;
        shouldShrink = false;
        shouldBlink = false;
    }

    public void ShouldShrink()
    {
        shouldShrink = true;
    }

    public void ShouldGrow()
    {
        shouldGrow = true;
    }

    public void ShouldBlink()
    {
        shouldBlink = true;
    }

    public void SetTrailColor(Color color)
    {
        color.a = 1.0f;
        // First Trail Mat. Colour
        Color c0A = color;
        Color c0B = TrailPalette.Generate(color, -0.015f, +0.02f, -0.10f);

        // Second Trail Mat. Colour
        Color c1A = TrailPalette.Generate(color, -0.035f, +0.30f, 0.00f);
        Color c1B = c1A;

        m_trailMaterials[0].SetColor("_Color01", c0A);
        m_trailMaterials[0].SetColor("_Color02", c0B);

        m_trailMaterials[1].SetColor("_Color01", c1A);
        m_trailMaterials[1].SetColor("_Color02", c1B);
    }


#region FSM
    private void EvaluateAnimationState()
    {
        switch (state)
        {
            case AnimationState.Idle:
                if (shouldShrink)
                    EnterShrink();
                if (shouldBlink)
                    EnterBlink();
                break;

            case AnimationState.Shrink:
                // Waiting for tween callback → no polling
                break;

            case AnimationState.Shrunk:
                if (shouldGrow)
                    EnterGrow();
                break;

            case AnimationState.Grow:
                // Waiting for tween callback → no polling
                break;
            case AnimationState.Blink:                    
                // Wainting for tween callback
                break;
        }
    }

    private void EnterShrink()
    {
        state = AnimationState.Shrink;
        scaleTween.PlayBackwards();
        trailScaleTween.PlayBackwards();
    }
    private void OnShrinkComplete()
    {
        state = AnimationState.Shrunk;
        EnableTrail(true);
        shouldShrink = false;
    }

    private void EnterGrow()
    {
        state = AnimationState.Grow;
        scaleTween.PlayForward();
        trailScaleTween.PlayForward();
    }
    private void OnGrowComplete()
    {
        state = AnimationState.Idle;
        EnableTrail(true);
        shouldGrow = false;
    }

    private void EnterBlink()
    {
        state = AnimationState.Blink;

        if (renderers == null)
            renderers = visualsParent.GetComponentsInChildren<Renderer>();
        EnableTrail(false);

        blinkTween?.Kill();

        // Inner looped blink sequence
        Sequence blinkLoop = DOTween.Sequence()
            .AppendCallback(() => SetRenderers(false))
            .AppendInterval(blinkDuration)
            .AppendCallback(() => SetRenderers(true))
            .AppendInterval(blinkDuration)
            .SetLoops(blinkCount * 2);

        // Master sequence (runs once)
        blinkTween = DOTween.Sequence()
            .AppendInterval(blinkDelay)
            .Append(blinkLoop)
            .SetLink(gameObject)
            .OnComplete(() => OnBlinkCompleted());
    }
    private void OnBlinkCompleted()
    {
        state = AnimationState.Idle;
        shouldBlink = false;
        EnableTrail(true);
    }

#endregion

    private void EnableTrail(bool value)
    {
        m_trail.SetActive(value);
        m_isTrailActive = value;
    }

    private void SetRenderers(bool value)
    {
        foreach (var r in renderers)
            r.enabled = value;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("DeadZone"))
        {
            m_trail.SetActive(false);
            m_isTrailActive = false;
        }
    }
}

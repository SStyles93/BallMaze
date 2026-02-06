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
    private bool isBlinking = false;
    private float startDelay = 0.5f;
    private Vector3 originalScale;
    private float originalTrailWidth;


    private enum ScaleState
    {
        Normal,
        Shrunk
    }

    private ScaleState state = ScaleState.Normal;

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
            .OnComplete(EnableTrail);

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
        transform.localScale = Vector3.zero;
        state = ScaleState.Shrunk;
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

        bool shouldShrink =
            playerMovement.State == PlayerState.IsFalling;

        // --- SHRINK ---
        if (shouldShrink && state == ScaleState.Normal)
        {
            Shrink();
        }
        else if (!shouldShrink && state == ScaleState.Shrunk)
        {
            Grow();
        }

        // --- BLINK ---
        if (playerMovement.State == PlayerState.IsDying)
        {
            Blink();
        }
        else if (isBlinking)
        {
            StopBlink();
        }
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

    public void SetPlayerShrunk()
    {
        transform.localScale = Vector3.zero;
        state = ScaleState.Shrunk;
        scaleTween.Rewind();
        trailScaleTween.Rewind();
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

    private void EnableTrail()
    {
        m_trail.SetActive(true);
        m_isTrailActive = true;
    }

    private void Shrink()
    {
        state = ScaleState.Shrunk;
        scaleTween.PlayBackwards();
        trailScaleTween.PlayBackwards();
    }

    private void Grow()
    {
        state = ScaleState.Normal;
        scaleTween.PlayForward();
        trailScaleTween.PlayForward();
    }

    private void Blink()
    {
        if (isBlinking)
            return;

        isBlinking = true;

        if (renderers == null)
            renderers = visualsParent.GetComponentsInChildren<Renderer>();

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
            .OnComplete(() => BlinkingEnded());
    }

    private void SetRenderers(bool value)
    {
        foreach (var r in renderers)
            r.enabled = value;
    }

    private void BlinkingEnded()
    {
        isBlinking = false;
    }

    private void StopBlink()
    {
        blinkTween?.Kill();
        SetRenderers(true);
        isBlinking = false;
    }

    private void SetAlpha(float alpha)
    {
        foreach (var r in renderers)
        {
            foreach (var mat in r.materials)
            {
                if (mat.HasProperty("_Color"))
                {
                    Color c = mat.color;
                    c.a = alpha;
                    mat.color = c;
                }
            }
        }
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

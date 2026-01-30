using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.Rendering;

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
    private Renderer[] renderers;
    private TrailRenderer[] trailRenderers;
    private Tween trailWidthTween;
    private bool m_isTrailActive = true;

    private bool isBlinking = false;

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
        if (GameStateManager.Instance?.CurrentGameState != GameState.Playing) return;

        bool shouldShrink =
            playerMovement.State == PlayerMovement.PlayerState.IsFalling;

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
        if (playerMovement.State == PlayerMovement.PlayerState.IsDying)
        {
            Blink();
        }
        else if(isBlinking)
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


    private void Shrink()
    {
        state = ScaleState.Shrunk;
        scaleTween.PlayForward();
        TweenTrailWidth(0f);
    }

    private void Grow()
    {
        state = ScaleState.Normal;
        scaleTween.PlayBackwards();
        TweenTrailWidth(1f);
    }

    private void TweenTrailWidth(float targetWidth)
    {
        trailWidthTween?.Kill();

        trailWidthTween = DOTween.To(
            () => trailRenderers[0].widthMultiplier,
            value =>
            {
                foreach (var tr in trailRenderers)
                    tr.widthMultiplier = value;
            },
            targetWidth,
            shrinkDuration
        ).SetEase(Ease.InOutQuad)
         .SetLink(gameObject);
    }

    private void EnableTrail()
    {
        m_trail.SetActive(true);
        m_isTrailActive = true;
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

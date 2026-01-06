using UnityEngine;
using DG.Tweening;

public class PlayerVisualEffects : MonoBehaviour
{
    [SerializeField] private float shrinkDuration = 1f;
    [SerializeField] private GameObject m_trail;
    [SerializeField] private Color m_trailColor;
    [SerializeField] private Material[] m_trailMaterials;


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

    public void SetTrailColor(Color color)
    {
        color.a = 1.0f;
        // First Trail Mat. Colour
        Color c1A = color;    
        Color c1B = TrailPalette.Generate(color, -0.015f, +0.02f, -0.10f);
        
        // Second Trail Mat. Colour
        Color c2A = TrailPalette.Generate(color, +0.215f, -0.33f, 0.00f);
        Color c2B = TrailPalette.Generate(color, -0.035f, +0.05f, -0.19f);

        m_trailMaterials[0].SetColor("_Color01", c1A);
        m_trailMaterials[0].SetColor("_Color02", c1B);

        m_trailMaterials[1].SetColor("_Color01", c2A);
        m_trailMaterials[1].SetColor("_Color02", c2B);
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
        m_trail.SetActive(true);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("DeadZone"))
        {
            m_trail.SetActive(false);
        }
    }
}

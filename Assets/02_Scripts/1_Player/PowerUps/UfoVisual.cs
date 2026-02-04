using UnityEngine;

public class UfoVisual : MonoBehaviour
{
    [Header("Blink Settings")]
    public float duration = 10f;
    public float minIntensity = 0f;
    public float maxIntensity = 5f;

    [Tooltip("Controls how blinking accelerates over time")]
    public AnimationCurve blinkAcceleration = AnimationCurve.EaseInOut(0, 0.2f, 1, 8f);

    private Material material;
    private Color baseEmissionColor;

    private float timer;
    private float phase;

    void Awake()
    {
        material = GetComponent<Renderer>().material;

        if (!material.IsKeywordEnabled("_EMISSION"))
            material.EnableKeyword("_EMISSION");

        baseEmissionColor = material.GetColor("_EmissionColor");
    }

    void OnEnable()
    {
        timer = 0f;
        phase = 0f;
    }

    void Update()
    {
        if (timer >= duration)
        {
            SetEmission(0f);
            enabled = false;
            return;
        }

        timer += Time.deltaTime;
        float t = timer / duration;

        // Frequency ramps up over time
        float frequency = blinkAcceleration.Evaluate(t);

        // Integrate frequency -> real acceleration
        phase += frequency * Time.deltaTime * Mathf.PI * 2f;

        float blink = Mathf.Sin(phase);
        blink = Mathf.InverseLerp(-1f, 1f, blink);

        float intensity = Mathf.Lerp(minIntensity, maxIntensity, blink);
        SetEmission(intensity);
    }

    private void SetEmission(float intensity)
    {
        material.SetColor("_EmissionColor", baseEmissionColor * intensity);
    }
}
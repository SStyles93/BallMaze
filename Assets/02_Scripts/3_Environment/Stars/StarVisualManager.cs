using UnityEngine;

public class StarVisualManager : MonoBehaviour
{
    [SerializeField] private Light lightref;
    [SerializeField] private Vector2 minMaxIntensityValue = new Vector2(0.5f, 1.0f);
    [SerializeField] private float sinePeriod = 2.0f;

    public void Update()
    {
        lightref.intensity = SineWave.SineWaveEffect(sinePeriod, minMaxIntensityValue.x, minMaxIntensityValue.y);
    }
}
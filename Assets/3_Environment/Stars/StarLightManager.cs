using UnityEngine;

public class StarLightManager : MonoBehaviour
{
    [SerializeField] private Light lightRef;
    [SerializeField] private Vector2 minMaxIntensityValue = new Vector2(0.5f, 1.0f);
    [SerializeField] private float sinePeriod = 2.0f;

    private void Start()
    {
        if(lightRef == null) lightRef = GetComponentInChildren<Light>();
    }

    public void Update()
    {
        lightRef.intensity = SineWave.SineWaveEffect(sinePeriod, minMaxIntensityValue.x, minMaxIntensityValue.y);
    }
}
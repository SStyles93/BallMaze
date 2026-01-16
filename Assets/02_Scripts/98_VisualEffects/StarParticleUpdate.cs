using UnityEngine;

public class StarParticleUpdate : MonoBehaviour
{
    private ParticleSystem starParticleSystem;

    private ParticleSystem.MinMaxGradient starColor;

    void Awake()
    {
        starParticleSystem = GetComponent<ParticleSystem>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        starColor = starParticleSystem.main.startColor;
        GetComponent<ParticleSystemRenderer>().material.SetColor("_EmissionColor", starColor.color * 4.0f);
    }
}

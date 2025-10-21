using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureSineWave : MonoBehaviour
{

    public float period = 2.0f;
    private float alphaFactor;
    private Color color;

    [SerializeField] private float colorChangeTimer = 3.0f;
    [SerializeField] int cycleCount = 0;
    [SerializeField] private Color blueColor;
    [SerializeField] private Color whiteColor;
    [SerializeField] private Color redColor;

    // Start is called before the first frame update
    void Start()
    {
        color = GetComponent<Image>().color;
    }

    // Update is called once per frame
    void Update()
    {

        if (period <= Mathf.Epsilon) { return; }
        float cycle = Time.time / period;
        colorChangeTimer -= Time.deltaTime;
        if (colorChangeTimer <= 0)
        {
            cycleCount++;
            ResetColorChangeTimer();
        }
        if (cycleCount > 2)
        {
            cycleCount = 0;
        }
        const float tau = Mathf.PI * 2.0f;
        float sineWave = Mathf.Sin(cycle * tau);
        alphaFactor = ((sineWave + 1.5f) / 3.0f); //SineWave = -1 to 1 // +1 to go from 0 to 2 // divided by 2 for 0 to 1
        switch (cycleCount)
        {
            case 0:
                color = blueColor;
                break;
            case 1:
                color = whiteColor;
                break;
            case 2:
                color = redColor;
                break;
            default:
                break;
        }
        color.a = alphaFactor;
        GetComponent<Image>().color = color;

    }

    private void ResetColorChangeTimer()
    {
        colorChangeTimer = 3.0f;
    }
}
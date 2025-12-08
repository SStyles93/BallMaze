using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SineWave : MonoBehaviour
{
    /// <summary>
    /// Returns a value between 0 and 1 (sine)
    /// </summary>
    /// <param name="period">The period of a revolution (0-1-0)</param>
    /// <returns></returns>
    public static float SineWaveEffect(float period)
    {
        if (period <= Mathf.Epsilon) return 0;

        float cycle = Time.time / period;
        const float tau = Mathf.PI * 2.0f;
        float sineWave = Mathf.Sin(cycle * tau);

        //SineWave = -1 to 1 -> (+1) to go from 0 to 2 -> (/2) for 0 to 1
        float sineWaveValue = (sineWave + 1.0f) / 2.0f; 

        return sineWaveValue;
    }

    public static float SineWaveEffect(float period, float minValue, float maxValue)
    {
        if (period <= Mathf.Epsilon)
            return minValue;

        float cycle = Time.time / period;
        const float tau = Mathf.PI * 2.0f;

        // Normal sine wave from -1..1
        float sine = Mathf.Sin(cycle * tau);

        // Convert from -1..1 → 0..1
        float normalized = (sine + 1f) * 0.5f;

        // Convert from 0..1 → minValue..maxValue
        float mappedSineWaveValue = Mathf.Lerp(minValue, maxValue, normalized);

        return mappedSineWaveValue;
    }
}
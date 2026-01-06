using UnityEngine;

public static class TrailPalette
{
    public static Color Generate(Color baseColor,
    float hueOffset,float satOffset,float valueOffset)
    {
        // Convert Linear → Gamma BEFORE HSV
        Color gammaColor = baseColor.gamma;

        Color.RGBToHSV(gammaColor, out float h, out float s, out float v);

        h = Mathf.Repeat(h + hueOffset, 1f);
        s = Mathf.Clamp01(s + satOffset);
        v = Mathf.Clamp01(v + valueOffset);

        // HSV → Gamma RGB
        Color gammaResult = Color.HSVToRGB(h, s, v);

        // Convert back to Linear for URP
        return gammaResult.linear;
    }
}

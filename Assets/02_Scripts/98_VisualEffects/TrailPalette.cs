using UnityEngine;

public static class TrailPalette
{
    public static Color Generate(Color baseColor,
        float hueOffset,float satMultiplier,
        float valueMultiplier,bool allowHDR = false)
    {
        Color.RGBToHSV(baseColor, out float h, out float s, out float v);

        h = Mathf.Repeat(h + hueOffset, 1f);
        s = Mathf.Clamp01(s * satMultiplier);

        v *= valueMultiplier;

        if (!allowHDR)
            v = Mathf.Clamp01(v);

        return Color.HSVToRGB(h, s, v);
    }
}

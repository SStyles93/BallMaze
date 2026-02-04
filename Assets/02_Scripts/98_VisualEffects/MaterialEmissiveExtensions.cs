using UnityEngine;

public static class MaterialEmissiveExtensions
{
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    /// <summary>
    /// Harmonizes the emissive color by forcing the HSV Value (V) channel.
    /// Preserves HDR intensity.
    /// </summary>
    public static void HarmonizeEmissive(this Material material, float targetSV, float targetEmissiveStrenght)
    {
        if (material == null)
            return;

        if (!material.HasProperty(EmissionColorID))
        {
            Debug.LogWarning($"Material '{material.name}' has no _EmissionColor property.");
            return;
        }

        if (!material.IsKeywordEnabled("_EMISSION"))
            return;

        // Get current emissive color (linear / HDR)
        Color emissive = material.GetColor(EmissionColorID);

        // Convert to HSV
        Color.RGBToHSV(emissive, out float h, out float s, out float v);

        // Override Value
        v = targetSV;
        s = targetSV;

        // Back to RGB
        Color newEmissive = Color.HSVToRGB(h, s, v);
        
        newEmissive *= targetEmissiveStrenght;

        material.SetColor(EmissionColorID, newEmissive);
        material.EnableKeyword("_EMISSION");
    }
}

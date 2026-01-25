using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerCustomization : MonoBehaviour
{
    [SerializeField] PlayerSkinData_SO playerSkinData_SO;

    [SerializeField] private Color m_color;
    [SerializeField] private GameObject m_visualContainer;
    [SerializeField] private MeshRenderer m_meshRenderer;
    [Range(0, 100)]
    [SerializeField] private int glassTintPercent = 15;
    [SerializeField] private float emissionStrenght = 2.0f;

    [SerializeField] private PlayerVisualEffects m_playerVisualEffects;

    private void OnEnable()
    {
        CustomizationManager.Instance.OnUpdatePlayerSkinOption += UpdateAppearence;
        CustomizationManager.Instance.OnOptionChanged += PreviewOption;
    }

    private void OnDisable()
    {
        CustomizationManager.Instance.OnUpdatePlayerSkinOption -= UpdateAppearence;
        CustomizationManager.Instance.OnOptionChanged -= PreviewOption;
    }

    private void Awake()
    {
        m_playerVisualEffects = transform.GetComponent<PlayerVisualEffects>();
    }

    void Start()
    {
        UpdateAppearence();
    }

    /// <summary>
    /// Sets the MeshRenderer to the SkinData_SO values
    /// </summary>
    public void UpdateAppearence()
    {
        // Clear, Create the saved skin
        InstanciateNewSkinPrefab(playerSkinData_SO.skinOption.skin);

        Color originalColor;
        // Sets the colour depending on if the skin is premium or not
        if (playerSkinData_SO.skinOption.isPremium)
        {
            originalColor = playerSkinData_SO.skinOption.color;
        }
        else
        {
            if (playerSkinData_SO.playerColorIndex == 0)
                originalColor = playerSkinData_SO.skinOption.skin.GetComponent<MeshRenderer>().sharedMaterial.color;
            else
                originalColor = playerSkinData_SO.colorOption.color;
        }

        m_meshRenderer.material.color = m_meshRenderer.sharedMaterial.name.Contains("Fresnel") ?
                TintedColourFrom(originalColor) : originalColor;
        if (m_meshRenderer.sharedMaterial.IsKeywordEnabled("_EMISSION"))
        {
            m_meshRenderer.material.SetColor("_EmissionColor", originalColor * emissionStrenght);
        }

        UpdateTrailColor(originalColor);
    }

    /// <summary>
    /// Sets the MeshRender directly to the option without passing by the SkinData_SO
    /// </summary>
    /// <param name="option">Option passed to be previewed</param>
    public void PreviewOption(CustomizationSlot slot)
    {
        switch (slot.option)
        {
            case ColorOption colorOpt:
                if(slot.index != 0)
                {
                    m_meshRenderer.material.color = m_meshRenderer.sharedMaterial.name.Contains("Fresnel") ?
                        TintedColourFrom(colorOpt.color) : colorOpt.color;
                    if (m_meshRenderer.sharedMaterial.IsKeywordEnabled("_EMISSION"))
                    {
                        m_meshRenderer.material.SetColor("_EmissionColor", colorOpt.color * emissionStrenght);
                    }
                }
                else // Special Case with "NONE" Colour
                {
                    Color originalColor = playerSkinData_SO.skinOption.skin.GetComponent<MeshRenderer>().sharedMaterial.color;
                    m_meshRenderer.material.color = m_meshRenderer.sharedMaterial.name.Contains("Fresnel") ?
                    TintedColourFrom(originalColor) : originalColor;
                }
                break;

            case SkinOption skinOpt:
                InstanciateNewSkinPrefab(skinOpt.skin);
                if (skinOpt.isPremium) m_meshRenderer.material.color = skinOpt.color;
                if (m_meshRenderer.sharedMaterial.IsKeywordEnabled("_EMISSION"))
                {
                    m_meshRenderer.material.SetColor("_EmissionColor", m_meshRenderer.material.color * emissionStrenght);
                }
                break;

            default:
                break;
        }
    }

    // --- PRIVATE METHODS ---

    private void InstanciateNewSkinPrefab(GameObject skinPrefab)
    {
        ClearVisualContainer();
        GameObject newVisual = Instantiate(skinPrefab, m_visualContainer.transform);
        m_meshRenderer = newVisual.GetComponent<MeshRenderer>();
    }
    private void UpdateTrailColor(Color color)
    {
        if (m_playerVisualEffects != null)
            m_playerVisualEffects.SetTrailColor(color);
    }

    private void ClearVisualContainer()
    {
        if (m_visualContainer == null) return;
        foreach (Transform child in m_visualContainer.GetComponentInChildren<Transform>())
        {
            Destroy(child.gameObject);
        }
    }

    private Color TintedColourFrom(Color color)
    {
        Color tintedColor = color;
        tintedColor *= (glassTintPercent / 100.0f);
        return tintedColor;
    }
}

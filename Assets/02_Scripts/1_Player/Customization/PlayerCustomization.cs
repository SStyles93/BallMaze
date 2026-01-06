using System;
using UnityEditor;
using UnityEngine;

public class PlayerCustomization : MonoBehaviour
{
    [SerializeField] PlayerSkinData_SO playerSkinData_SO;

    [SerializeField] private Color m_color;
    [SerializeField] private GameObject m_visualContainer;
    [SerializeField] private MeshRenderer m_meshRenderer;
    [Range(0,100)]
    [SerializeField] private int glassTintPercent = 15;

    [SerializeField] private PlayerVisualEffects m_playerVisualEffects;

    private void OnEnable()
    {
        CustomizationManager.Instance.OnUpdatePlayerOption += UpdateAppearence;
        CustomizationManager.Instance.OnOptionChanged += PreviewOption;
    }

    private void OnDisable()
    {
        CustomizationManager.Instance.OnUpdatePlayerOption -= UpdateAppearence;
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
        UpdateSkinPrefab(playerSkinData_SO.playerSkin);

        // if idx is 0 assign material colour
        if (playerSkinData_SO.playerColorIndex == 0)
        {
            Color newColor = TintedColourFrom(
                    playerSkinData_SO.playerSkin.GetComponent<MeshRenderer>().sharedMaterial.color);
            m_meshRenderer.material.color = newColor;
            UpdateTrailColor(newColor);
        }
        // Otherwise assign selected colour
        else
        {
            Color newColor = TintedColourFrom(playerSkinData_SO.playerColor);
            m_meshRenderer.material.color = newColor;
            UpdateTrailColor(newColor);
        }
    }

    /// <summary>
    /// Sets the MeshRender directly to the option without passing by tge SkinData_SO
    /// </summary>
    /// <param name="option">Option passed to be previewed</param>
    public void PreviewOption(CustomizationOption option)
    {
        switch (option)
        {
            case ColorOption colorOpt:
                m_meshRenderer.material.color = TintedColourFrom(colorOpt.color);
                break;

            case SkinOption skinOpt:
                UpdateSkinPrefab(skinOpt.skin);
                break;
        }
    }

    /// <summary>
    /// Assigns the Option to the MeshRender AND the SKinData_SO
    /// </summary>
    /// <param name="option">Option to pass</param>
    /// <param name="index">Index of the option to pass</param>
    public void AssignOption(CustomizationOption option, int index)
    {
        switch (option)
        {
            case ColorOption colorOpt:
                AssignColor(colorOpt.color);
                AssignColorIndex(index);
                break;

            case SkinOption skinOpt:
                UpdateSkinPrefab(skinOpt.skin);
                AssignSkin(skinOpt.skin);
                AssignMaterialIndex(index);
                Color colorToAssign = skinOpt.skin.GetComponent<MeshRenderer>().sharedMaterial.color;
                AssignColor(colorToAssign);
                AssignColorIndex(0);
                break;
        }
    }

    /// <summary>
    /// Assign the original color of the material
    /// </summary>
    public void AssignOriginalColor()
    {
        Color color = playerSkinData_SO.playerSkin.GetComponent<MeshRenderer>().sharedMaterial.color;
        playerSkinData_SO.playerColor = color;
        playerSkinData_SO.playerColorIndex = 0;
        UpdateAppearence();
    }

    // --- PRIVATE METHODS ---

    private void AssignColor(Color color)
    {
        playerSkinData_SO.playerColor = color;
        m_meshRenderer.material.color = TintedColourFrom(color);
    }
    private void AssignColorIndex(int index)
    {
        playerSkinData_SO.playerColorIndex = index;
    }

    private void AssignSkin(GameObject skin)
    {
        //m_meshRenderer.sharedMaterial = skin;
        playerSkinData_SO.playerSkin = skin;
    }
    private void AssignMaterialIndex(int index)
    {
        playerSkinData_SO.playerSkinIndex = index;
    }

    private void UpdateSkinPrefab(GameObject skinPrefab)
    {
        ClearVisualContainer();
        GameObject newVisual = Instantiate(skinPrefab, m_visualContainer.transform);
        m_meshRenderer = newVisual.GetComponent<MeshRenderer>();
    }
    private void UpdateTrailColor(Color color)
    {
        if(m_playerVisualEffects != null)
        m_playerVisualEffects.SetTrailColor(color);
    }

    private void ClearVisualContainer()
    {
        foreach(Transform child in m_visualContainer.GetComponentInChildren<Transform>())
        {
            Destroy(child.gameObject);
        }
    }

    private Color TintedColourFrom(Color color)
    {
        Color tintedColor = color;
        tintedColor.a = glassTintPercent/100.0f;
        return tintedColor;
    } 
}

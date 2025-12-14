using System;
using UnityEditor;
using UnityEngine;

public class PlayerCustomization : MonoBehaviour
{
    [SerializeField] PlayerSkinData_SO playerSkinData_SO;

    [SerializeField] private Color m_color;
    [SerializeField] private MeshRenderer m_meshRenderer;

    private void OnEnable()
    {
        ShopManager.Instance.OnUpdatePlayerOption += UpdateAppearence;
        ShopManager.Instance.OnOptionChanged += PreviewOption;
    }

    private void OnDisable()
    {
        ShopManager.Instance.OnUpdatePlayerOption -= UpdateAppearence;
        ShopManager.Instance.OnOptionChanged -= PreviewOption;
    }

    private void Awake()
    {
        m_meshRenderer ??= GetComponent<MeshRenderer>();
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
        // Assigne the saved material
        m_meshRenderer.material = playerSkinData_SO.playerMaterial;

        // if idx is 0 assign material colour
        if (playerSkinData_SO.playerColorIndex == 0)
            m_meshRenderer.material.color = playerSkinData_SO.playerMaterial.color;
        // Otherwise assign selected colour
        else
            m_meshRenderer.material.color = playerSkinData_SO.playerColor;


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
                m_meshRenderer.material.color = colorOpt.color;
                break;

            case MaterialOption materialOpt:
                m_meshRenderer.material = materialOpt.material;
                //TODO: Initial colour of the material option
                //m_meshRenderer.material.color = ;
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

            case MaterialOption materialOpt:
                AssignMaterial(materialOpt.material);
                AssignMaterialIndex(index);
                AssignColor(materialOpt.material.color);
                AssignColorIndex(0);
                break;
        }
    }

    /// <summary>
    /// Assign the original color of the material
    /// </summary>
    public void AssignOriginalColor()
    {
        playerSkinData_SO.playerColor = playerSkinData_SO.playerMaterial.color;
        playerSkinData_SO.playerColorIndex = 0;
        UpdateAppearence();
    }

    // --- PRIVATE METHODS ---

    private void AssignColor(Color color)
    {
        m_meshRenderer.material.color = color;
        playerSkinData_SO.playerColor = color;
    }
    private void AssignColorIndex(int index)
    {
        playerSkinData_SO.playerColorIndex = index;
    }

    private void AssignMaterial(Material material)
    {
        m_meshRenderer.sharedMaterial = material;
        playerSkinData_SO.playerMaterial = material;
    }
    private void AssignMaterialIndex(int index)
    {
        playerSkinData_SO.playerMaterialIndex = index;
    }
}

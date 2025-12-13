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
        m_meshRenderer.material = playerSkinData_SO.playerMaterial;
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
                break;
        }
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
        Color currentColor = m_meshRenderer.material.color; 
        m_meshRenderer.sharedMaterial = material;
        m_meshRenderer.material.color = currentColor;
        playerSkinData_SO.playerMaterial = material;      
    }
    private void AssignMaterialIndex(int index)
    {
        playerSkinData_SO.playerMaterialIndex = index;
    }
}

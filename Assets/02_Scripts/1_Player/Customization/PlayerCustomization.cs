using System;
using UnityEditor;
using UnityEngine;

public class PlayerCustomization : MonoBehaviour
{
    [SerializeField] PlayerSkinData_SO playerSkinData_SO;

    [SerializeField] private Color m_color;
    [SerializeField] private GameObject m_visualContainer;
    [SerializeField] private MeshRenderer m_meshRenderer;

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
        // Assigne the saved material - Skin
        // Clear Visuals
        //Instantiate new visuals.
        //m_meshRenderer.material = playerSkinData_SO.playerSkin;

        // if idx is 0 assign material colour
        if (playerSkinData_SO.playerColorIndex == 0)
            m_meshRenderer.material.color = playerSkinData_SO.playerSkin.GetComponent<MeshRenderer>().material.color;
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

            case SkinOption skinOpt:
                //Clear Visu.
                //Instantiate Visu.
                //m_meshRenderer.material = skinOpt.skin;
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
                AssignSkin(skinOpt.skin);
                AssignMaterialIndex(index);
                AssignColor(skinOpt.skin.GetComponent<MeshRenderer>().material.color);
                AssignColorIndex(0);
                break;
        }
    }

    /// <summary>
    /// Assign the original color of the material
    /// </summary>
    public void AssignOriginalColor()
    {
        playerSkinData_SO.playerColor = playerSkinData_SO.playerSkin.GetComponent<MeshRenderer>().material.color;
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

    private void AssignSkin(GameObject skin)
    {
        //m_meshRenderer.sharedMaterial = skin;
        playerSkinData_SO.playerSkin = skin;
    }
    private void AssignMaterialIndex(int index)
    {
        playerSkinData_SO.playerSkinIndex = index;
    }

    private void ClearVisualContainer()
    {
        foreach(Transform child in m_visualContainer.GetComponentInChildren<Transform>())
        {
            Destroy(child);
        }
    }
}

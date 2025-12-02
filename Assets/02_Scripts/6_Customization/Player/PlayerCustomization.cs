using UnityEditor;
using UnityEngine;

public class PlayerCustomization : MonoBehaviour
{
    [SerializeField] PlayerSkinData_SO playerSkinData_SO;

    [SerializeField] private Color m_color;
    [SerializeField] private MeshRenderer m_meshRenderer;

    private void Awake()
    {
        m_meshRenderer ??= GetComponent<MeshRenderer>();
    }

    void Start()
    {
        m_meshRenderer.material = playerSkinData_SO.playerMaterial;
        m_meshRenderer.material.color = playerSkinData_SO.playerColor;
    }

    public void AssignColor(Color color)
    {
        m_meshRenderer.material.color = color;
        playerSkinData_SO.playerColor = color;
    }
    public void AssignMaterial(Material material)
    {
        Color currentColor = m_meshRenderer.material.color; 
        m_meshRenderer.sharedMaterial = material;
        m_meshRenderer.material.color = currentColor;
        playerSkinData_SO.playerMaterial = material;      
    }
}

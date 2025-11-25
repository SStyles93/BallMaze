using UnityEngine;

public class PlayerCustomization : MonoBehaviour
{
    [SerializeField] PlayerSkinData_SO playerSkinData;

    [SerializeField] private MeshFilter m_meshFilter;
    [SerializeField] private MeshRenderer m_meshRenderer;

    private void Awake()
    {
        m_meshFilter ??= GetComponent<MeshFilter>();
        m_meshRenderer ??= GetComponent<MeshRenderer>();
    }

    void Start()
    {
        m_meshFilter.mesh = playerSkinData.playerMesh;
        m_meshRenderer.material = playerSkinData.playerMaterial;
    }

    void Update()
    {
        
    }
}

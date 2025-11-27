using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class MaterialOption
{
    public Material material;
    public Sprite sprite;
    public bool isLocked;
}

public class MaterialPannelInitializer : MonoBehaviour
{

    // Reference to the player customization
    [SerializeField] PlayerCustomization playerCustomization;

    [SerializeField] private GameObject materialPannel;
    [SerializeField] private GameObject materialSlotPrefab;

    [SerializeField] private List<MaterialOption> materials = new List<MaterialOption>();
    private void Start()
    {
        foreach (var option in materials)
        {
            GameObject materialSlot = Instantiate(materialSlotPrefab, materialPannel.transform);
            materialSlot.GetComponent<MaterialSlot>().InitializeMaterialSlot(option, playerCustomization);
        }
    }
}

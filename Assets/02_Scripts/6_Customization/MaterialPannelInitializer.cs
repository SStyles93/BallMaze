using System.Collections.Generic;
using UnityEngine;

public class MaterialPannelInitializer : MonoBehaviour
{

    // Reference to the player customization
    [SerializeField] PlayerCustomization playerCustomization;

    [SerializeField] private GameObject materialPannel;
    [SerializeField] private GameObject materialSlotPrefab;

    [SerializeField] private CustomizationData_SO customizationData_SO;

    private void Start()
    {
        foreach (var option in customizationData_SO.materials)
        {
            GameObject materialSlot = Instantiate(materialSlotPrefab, materialPannel.transform);
            materialSlot.GetComponent<MaterialSlot>().InitializeMaterialSlot(option, playerCustomization);
        }
    }
}

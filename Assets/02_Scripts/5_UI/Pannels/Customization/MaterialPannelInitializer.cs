using System.Collections.Generic;
using UnityEngine;

public class MaterialPannelInitializer : MonoBehaviour
{

    // Reference to the player customization
    [SerializeField] PlayerCustomization playerCustomization;

    [SerializeField] private GameObject materialSlotsContainer;
    [SerializeField] private GameObject materialSlotPrefab;

    [SerializeField] private CustomizationData_SO customizationData_SO;

    private void Start()
    {
        for (int i = 0; i < customizationData_SO.skins.Length; i++)
        {
            GameObject materialSlot = Instantiate(materialSlotPrefab, materialSlotsContainer.transform);
            materialSlot.GetComponent<MaterialSlot>().InitializeMaterialSlot(customizationData_SO.skins[i], i, playerCustomization);
        }
    }
}

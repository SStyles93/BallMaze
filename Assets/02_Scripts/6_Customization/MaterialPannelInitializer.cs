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
        for (int i = 0; i < customizationData_SO.materials.Length; i++)
        {
            GameObject materialSlot = Instantiate(materialSlotPrefab, materialPannel.transform);
            materialSlot.GetComponent<MaterialSlot>().InitializeMaterialSlot(customizationData_SO.materials[i], i, playerCustomization);
        }
    }
}

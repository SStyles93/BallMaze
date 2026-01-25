using System.Collections.Generic;
using UnityEngine;

public class MaterialPannelManager : MonoBehaviour
{
    [Header("Project References")]
    [SerializeField] private CustomizationData_SO customizationData_SO;

    [Header("Scene References")]
    [SerializeField] PlayerCustomization playerCustomization;
    [SerializeField] private GameObject materialSlotsContainer;
    [SerializeField] private GameObject materialSlotPrefab;


    private void Start()
    {
        for (int i = 0; i < customizationData_SO.skins.Length; i++)
        {
            GameObject materialSlot = Instantiate(materialSlotPrefab, materialSlotsContainer.transform);
            materialSlot.GetComponent<SkinSlot>().InitializeSkinSlot(customizationData_SO.skins[i], i, playerCustomization);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class ColorPannelInitializer : MonoBehaviour
{
    // Reference to the player customization
    [SerializeField] PlayerCustomization playerCustomization;

    [SerializeField] private GameObject colorPannel;
    [SerializeField] private GameObject colorSlotPrefab;

    [SerializeField] private CustomizationData_SO customizationData_SO;


    private void Start()
    {
        foreach (var option in customizationData_SO.colors)
        {
            GameObject colorSlot = Instantiate(colorSlotPrefab, colorPannel.transform);
            colorSlot.GetComponent<ColorSlot>().InitializeColorSlot(option, playerCustomization);
        }
    }
}

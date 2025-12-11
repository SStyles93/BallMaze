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
        for (int i = 0; i < customizationData_SO.colors.Length; i++)
        {
            GameObject colorSlot = Instantiate(colorSlotPrefab, colorPannel.transform);
            colorSlot.GetComponent<ColorSlot>().InitializeColorSlot(customizationData_SO.colors[i], i, playerCustomization);
        }
    }
}

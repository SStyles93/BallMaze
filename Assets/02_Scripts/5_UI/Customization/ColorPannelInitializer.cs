using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ColorOption 
{
    public Color color;
    public bool isLocked;
}

public class ColorPannelInitializer : MonoBehaviour
{
    // Reference to the player customization
    [SerializeField] PlayerCustomization playerCustomization;

    [SerializeField] private GameObject colorPannel;
    [SerializeField] private GameObject colorSlotPrefab;

    [SerializeField] private List<ColorOption> colors = new List<ColorOption>();


    private void Start()
    {
        foreach (var option in colors)
        {
            GameObject colorSlot = Instantiate(colorSlotPrefab, colorPannel.transform);
            colorSlot.GetComponent<ColorSlot>().InitializeColorSlot(option, playerCustomization);
        }
    }
}

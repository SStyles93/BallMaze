using UnityEngine;
using UnityEngine.EventSystems;

public class NONEColorSlot : CustomizationSlot
{
    [SerializeField] private PlayerCustomization playerCustomizationRef;

    private void Start()
    {
        playerCustomization = playerCustomizationRef;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        // If the click released when over the slot
        if (!eventData.dragging && isMouseOver)
        {
            // Option is assigned to the player (is unlocked)
            playerCustomization.AssignOriginalColor();
            //Debug.Log($"PointerUp on {this.gameObject.name}");
        }
    }
}

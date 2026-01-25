using UnityEngine;
using UnityEngine.EventSystems;

public class NONEColorSlot : CustomizationSlot
{
    [SerializeField] private PlayerCustomization playerCustomizationRef;
    [SerializeField] private CustomizationData_SO customizationData_SO;

    private void Start()
    {
        playerCustomization = playerCustomizationRef;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        // If the click released when over the slot
        if (!eventData.dragging && isMouseOver)
        {
            this.index = 0;
            this.option = customizationData_SO.colors[0];
            this.isLocked = false;
            // Option is assigned to the player (is unlocked)
            CustomizationManager.Instance.SetCurrentCustomizationSlot(this);
            //Debug.Log($"PointerUp on {this.gameObject.name}");
        }
    }
}

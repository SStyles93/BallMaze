using UnityEngine.EventSystems;


public class MaterialSlot : CustomizationSlot
{
    private MaterialOption skinData;

    public void InitializeMaterialSlot(MaterialOption option, PlayerCustomization playerCustomization)
    {
        InitializeSlot(option, playerCustomization);

        skinData = option;
        slotImage.sprite = option.sprite;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.dragging)
        {
            if (isLocked)
            {
                lockImage.color = lockColor;
                return;
            }

            //Debug.Log($"Pointer was dragged on {this.gameObject.name}");
        }
        else // when the element is not dragged
        {
            if (isLocked) return;
            playerCustomization.AssignMaterial(skinData.material);
            //Debug.Log($"PointerUp on {this.gameObject.name}");
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (isLocked)
        {
            //Try to buy
            return;
        }
        //Debug.Log($"PointerDown on {this.gameObject.name}");
    }
}

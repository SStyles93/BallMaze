using UnityEngine;
using UnityEngine.EventSystems;

public class ColorSlot : CustomizationSlot
{
    public ColorOption colorOption;

    private Color m_lockedColor;

    public void InitializeColorSlot(ColorOption option, int optionIndex, PlayerCustomization playerCustomization)
    {
        InitializeSlot(option, optionIndex, playerCustomization);

        colorOption = option;
        slotImage.color = option.color;

        m_lockedColor = colorOption.color;
        m_lockedColor.a = .5f;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if (eventData.dragging)
        {
            if (isLocked)
            {
                lockImage.color = lockColor;
                slotImage.color = m_lockedColor;
                return;
            }

            slotImage.color = colorOption.color;
            //Debug.Log($"Pointer was dragged on {this.gameObject.name}");
        }
        else if(isMouseOver) // when the element is not dragged
        {
            if (isLocked)
            {
                ShopManager.Instance.SetCurrentCustomizationSlot(this);
                return;
            }

            playerCustomization.AssignColor(colorOption.color);
            playerCustomization.AssignColorIndex(index);
            //Debug.Log($"PointerUp on {this.gameObject.name}");
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (isLocked) return;
        lockImage.color = colorOption.color;
        //Debug.Log($"PointerDown on {this.gameObject.name}");
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class ColorSlot : CustomizationSlot
{
    [SerializeField] private Color slotColor;

    private Color m_lockedColor;

    public void InitializeColorSlot(ColorOption option, PlayerCustomization playerCustomization)
    {
        InitializeSlot(option, playerCustomization);

        slotColor = option.color;
        slotImage.color = option.color;

        m_lockedColor = slotColor;
        m_lockedColor.a = 50f;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.dragging)
        {

            if (isLocked)
            {
                lockImage.color = lockColor;
                slotImage.color = m_lockedColor;
                return;
            }

            slotImage.color = slotColor;
            //Debug.Log($"Pointer was dragged on {this.gameObject.name}");
        }
        else // when the element is not dragged
        {
            if (isLocked) return;
            // TODO: ASSIGN COLOR
            playerCustomization.AssignColor(slotColor);
            //Debug.Log($"PointerUp on {this.gameObject.name}");
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (isLocked) return;
        lockImage.color = slotColor;
        //Debug.Log($"PointerDown on {this.gameObject.name}");
    }
}

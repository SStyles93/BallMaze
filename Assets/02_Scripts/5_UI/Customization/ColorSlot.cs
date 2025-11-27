using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorSlot : BaseUISlot
{
    [SerializeField] private Color slotColor;

    private Color m_lockedColor;

    private PlayerCustomization playerCustomization;

    public void InitializeColorSlot(ColorOption option, PlayerCustomization playerCustomization)
    {
        slotColor = option.color;
        slotImage.color = option.color;
        m_lockedColor = slotColor;
        m_lockedColor.a = .5f;

        this.playerCustomization = playerCustomization;

        if (option.isLocked)
        {
            this.isLocked = option.isLocked;
            lockImage.sprite = lockSprite;
            lockImage.color = lockColor;
        }
        else
        {
            lockImage.enabled = false;
        }
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

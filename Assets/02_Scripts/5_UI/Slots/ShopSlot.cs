using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopSlot : BaseUISlot
{
    [Header("ShopSlot Parameters")]
    [SerializeField] private Sprite coinStackSprite;
    [SerializeField] private TMP_Text valueText;

    private ShopOption shopOption;
    private ShopManager shopManager;

    public virtual void InitializeSlot(ShopOption shopOption, ShopManager shopManager)
    {
        this.shopOption = shopOption;
        this.shopManager = shopManager;

        slotImage.GetComponent<Image>().sprite = coinStackSprite;
        valueText.text = $"{shopOption.price.value} {shopOption.price.currencyType.ToString()}";
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        // If click released when pointer is dragged
        if (eventData.dragging)
        {
            if (isLocked)
            {
                lockImage.color = lockColor;
                return;
            }

            //Debug.Log($"Pointer was dragged on {this.gameObject.name}");
        }
        // If the click released when over the slot
        else if (isMouseOver)
        {
            // Material is given to shop manager (not yet unlocked)
            if (isLocked)
            {
                return;
            }


            //Debug.Log($"PointerUp on {this.gameObject.name}");
        }
    }
}

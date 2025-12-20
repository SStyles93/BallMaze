using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopSlot : MonoBehaviour
{
    // Slot parameters
    [Header("Slot Parameters")]
    [SerializeField] protected Image slotImage;

    [Header("ShopSlot Parameters")]
    [SerializeField] private Sprite coinStackSprite;
    [SerializeField] private TMP_Text coinAmountText;
    [SerializeField] private TMP_Text valueText;

    private ShopOption shopOption;
    private ShopManager shopManager;

    public virtual void InitializeSlot(ShopOption shopOption, ShopManager shopManager)
    {
        this.shopOption = shopOption;
        this.shopManager = shopManager;

        slotImage.GetComponent<Image>().sprite = coinStackSprite;
        coinAmountText.text = shopOption.currencyAmountPairs[0].Amount.ToString();
        valueText.text = $"{shopOption.price.value} {shopOption.price.currencyType.ToString()}";
    }

    public virtual void SendSlotDataToManager() 
    {
        if (shopManager != null)
        {
            shopManager.ProcessShopOption(shopOption);
        }
    }
}

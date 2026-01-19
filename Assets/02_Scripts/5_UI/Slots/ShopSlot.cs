using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    [Header("Slot Parameters")]
    [SerializeField] private Image slotImage;

    [Header("ShopSlot Parameters")]
    [SerializeField] private TMP_Text coinAmountText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Button buyButton;

    private ProductCatalogItem catalogItem;
    private ShopManager shopManager;

    // ---------------- INITIALIZATION ----------------

    public ShopSlot InitializeFromCatalog(ProductCatalogItem item, ShopManager manager)
    {
        catalogItem = item;
        shopManager = manager;

        buyButton.interactable = true;

        SetPayoutText(item);

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);

        return this;
    }

    // ---------------- RUNTIME PRODUCT BIND ----------------

    public ShopSlot SetProductPrice(List<Product> products)
    {
        foreach (var product in products)
        {
            if (product.definition.id != catalogItem.id)
                continue;
            valueText.text = product.metadata.localizedPriceString;
            buyButton.interactable = product.availableToPurchase;
        }
        return this;
    }

    // ---------------- UI HELPERS ----------------

    void SetPayoutText(ProductCatalogItem item)
    {
        foreach (var payout in item.Payouts)
        {
            if (payout.type ==
                ProductCatalogPayout.ProductCatalogPayoutType.Currency)
            {
                coinAmountText.text = $"{payout.quantity} <sprite index=0>";
                return;
            }
        }

        coinAmountText.text = "?";
    }

    // ---------------- ACTION ----------------

    void OnBuyClicked()
    {
        shopManager.Buy(catalogItem.id);
    }
}

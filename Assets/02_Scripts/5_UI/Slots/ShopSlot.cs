using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    [Header("ShopSlot Parameters")]
    [SerializeField] private TMP_Text coinAmountText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Button buyButton;

    [SerializeField] private string Id = "TYPE_SIZE_OTHER";

    // ---------------- INITIALIZATION ----------------

    private void Start()
    {
        foreach (var product in ShopIAPManager.Instance.Products)
        {
            if (product.definition.id != Id) continue;

            this.Initialize()
                .SetProductPrice(product);
        }
    }


    public ShopSlot Initialize()
    {
        buyButton.interactable = true;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);

        return this;
    }

    // ---------------- RUNTIME PRODUCT BIND ----------------

    public ShopSlot SetProductPrice(Product product)
    {
        valueText.text = product.metadata.localizedPriceString;
        buyButton.interactable = product.availableToPurchase;
        return this;
    }

// ---------------- ACTION ----------------

void OnBuyClicked()
{
    ShopIAPManager.Instance.BuyProduct(Id);
}
}

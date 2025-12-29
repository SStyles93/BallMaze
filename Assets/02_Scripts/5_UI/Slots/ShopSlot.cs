using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;

public class ShopSlot : MonoBehaviour
{
    [Header("Slot Parameters")]
    [SerializeField] private Image slotImage;

    [Header("ShopSlot Parameters")]
    [SerializeField] private Sprite coinStackSprite;
    [SerializeField] private TMP_Text coinAmountText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Button buyButton;

    private ProductCatalogItem catalogItem;
    private Product runtimeProduct;
    private ShopManager shopManager;

    // ---------------- INITIALIZATION ----------------

    public void InitializeFromCatalog(ProductCatalogItem item, ShopManager manager)
    {
        catalogItem = item;
        shopManager = manager;

        slotImage.sprite = coinStackSprite;
        valueText.text = "...";
        buyButton.interactable = false;

        SetPayoutText(item);

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    // ---------------- RUNTIME PRODUCT BIND ----------------

    public void BindRuntimeProduct(Product product)
    {
        if (product.definition.id != catalogItem.id)
            return;

        runtimeProduct = product;

        valueText.text = product.metadata.localizedPriceString;
        buyButton.interactable = product.availableToPurchase;
    }

    // ---------------- UI HELPERS ----------------

    void SetPayoutText(ProductCatalogItem item)
    {
        foreach (var payout in item.Payouts)
        {
            if (payout.type ==
                ProductCatalogPayout.ProductCatalogPayoutType.Currency)
            {
                coinAmountText.text = payout.quantity.ToString();
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

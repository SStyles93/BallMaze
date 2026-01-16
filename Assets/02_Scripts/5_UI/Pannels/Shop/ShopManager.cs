using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject shopSlotPrefab;
    [SerializeField] private Transform shopSlotsContainer;

    private ProductCatalog catalog;

    private void Start()
    {
        catalog = ProductCatalog.LoadDefaultCatalog();
        CreateShopSlots();
    }

    private void CreateShopSlots()
    {
        foreach (var product in catalog.allProducts)
        {
            GameObject slotGO = Instantiate(
                shopSlotPrefab,
                shopSlotsContainer
            );

            slotGO.GetComponent<ShopSlot>()
                .InitializeFromCatalog(product, this)
                .SetProductPrice(ShopIAPManager.Instance.Products);
        }
    } 

    public void Buy(string productId)
    {
        ShopIAPManager.Instance.BuyProduct(productId);
    }
}

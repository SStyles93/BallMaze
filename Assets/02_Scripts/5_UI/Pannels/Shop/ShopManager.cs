using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> shopSlotPrefabs;
    [SerializeField] private Transform shopSlotsContainer;

    private ProductCatalog catalog;

    private void Start()
    {
        catalog = ProductCatalog.LoadDefaultCatalog();
        CreateShopSlots();
    }

    private void CreateShopSlots()
    {
        int productIndex = 0;

        foreach (var product in catalog.allProducts)
        {
            GameObject slotGO = Instantiate(
                shopSlotPrefabs[productIndex],
                shopSlotsContainer
            );

            slotGO.GetComponent<ShopSlot>()
                .InitializeFromCatalog(product, this)
                .SetProductPrice(ShopIAPManager.Instance.Products);

            productIndex++;
        }
    } 

    public void Buy(string productId)
    {
        ShopIAPManager.Instance.BuyProduct(productId);
    }
}

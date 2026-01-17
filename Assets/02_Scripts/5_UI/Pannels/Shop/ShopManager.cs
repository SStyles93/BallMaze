using UnityEngine;
using UnityEngine.Purchasing;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject shopSlotPrefab;
    [SerializeField] private Transform shopSlotsContainer;

    [SerializeField] private Sprite[] coinSprites = new Sprite[5];

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
                shopSlotPrefab,
                shopSlotsContainer
            );

            slotGO.GetComponent<ShopSlot>()
                .InitializeFromCatalog(product, this, coinSprites[productIndex])
                .SetProductPrice(ShopIAPManager.Instance.Products);

            productIndex++;
        }
    } 

    public void Buy(string productId)
    {
        ShopIAPManager.Instance.BuyProduct(productId);
    }
}

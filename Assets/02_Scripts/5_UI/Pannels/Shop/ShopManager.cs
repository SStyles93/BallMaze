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

    private void OnEnable()
    {
        ShopIAPManager.Instance.OnProductsFetchedEvent += OnProductsFetched;
    }

    private void OnDisable()
    {
        if (ShopIAPManager.Instance != null)
        {
            ShopIAPManager.Instance.OnProductsFetchedEvent -= OnProductsFetched;
        }
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
                .InitializeFromCatalog(product, this);
        }
    }

    private void OnProductsFetched(Product[] products)
    {
        foreach (Transform child in shopSlotsContainer)
        {
            var slot = child.GetComponent<ShopSlot>();
            if (slot == null) continue;

            foreach (var product in products)
            {
                slot.BindRuntimeProduct(product);
            }
        }
    }

    public void Buy(string productId)
    {
        ShopIAPManager.Instance.BuyProduct(productId);
    }
}

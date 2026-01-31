using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

public class ShopIAPManager : MonoBehaviour
{
    public static ShopIAPManager Instance { get; private set; }

    private StoreController storeController;
    private ProductCatalog catalog;
    private List<Product> products;

    private bool isConnected;
    public bool IsInitialized => storeController != null && isConnected;

    public List<Product> Products { get => products; set => products = value; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);

        InitializeIAP();
    }

    async void InitializeIAP()
    {
        catalog = ProductCatalog.LoadDefaultCatalog();
        storeController = UnityIAPServices.StoreController();

        storeController.OnPurchasePending += OnPurchasePending;
        storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
        storeController.OnPurchaseFailed += OnPurchaseFailed;

        storeController.OnProductsFetched += OnProductsFetched;
        storeController.OnProductsFetchFailed += OnProductsFetchFailed;
        storeController.OnStoreDisconnected += OnStoreDisconnected;

        //Debug.Log("Connecting to store...");
        await storeController.Connect();
        isConnected = true;

        FetchProductsFromCatalog();
    }

    // --- FETCH PRODUCTS ---

    public void FetchProductsFromCatalog()
    {
        var productsToFetch = new List<ProductDefinition>();

        foreach (var item in catalog.allProducts)
        {
            productsToFetch.Add(new ProductDefinition(item.id, item.type));
        }

        if (productsToFetch.Count == 0)
        {
            Debug.LogWarning("IAP Catalog is empty.");
            return;
        }

        storeController.FetchProducts(productsToFetch);
    }

    // --- BUY ---

    public void BuyProduct(string productId)
    {
        if (!IsInitialized)
        {
            Debug.LogWarning("IAP not initialized.");
            return;
        }

        storeController.PurchaseProduct(productId);
    }

    // --- PURCHASE FLOW ---

    void OnPurchasePending(PendingOrder order)
    {
        var product = GetFirstProduct(order);
        if (product == null)
        {
            Debug.LogWarning("Pending order contains no product.");
            return;
        }

        //Debug.Log($"Purchase pending: {product.definition.id}");
        storeController.ConfirmPurchase(order);
    }

    void OnPurchaseConfirmed(Order order)
    {
        if (order is ConfirmedOrder)
        {
            //Debug.Log("Purchase confirmed.");

            var product = GetFirstProduct(order);
            GrantPayouts(product.definition.id);
            SavingManager.Instance.SavePlayer();
        }
        else if (order is FailedOrder failed)
        {
            Debug.LogError(
                $"Purchase confirmation failed: {failed.FailureReason} - {failed.Details}"
            );
        }
    }

    void OnPurchaseFailed(FailedOrder order)
    {
        var product = GetFirstProduct(order);

        Debug.LogError(
            $"Purchase failed - Product: {product?.definition.id}, " +
            $"Reason: {order.FailureReason}, Details: {order.Details}"
        );
    }

    // --- PAYOUTS ---

    void GrantPayouts(string productId)
    {
        var catalogItem = catalog.allProducts
            .FirstOrDefault(p => p.id == productId);

        if (catalogItem == null)
        {
            Debug.LogWarning($"No catalog item for product {productId}");
            return;
        }

        foreach (var payout in catalogItem.Payouts)
        {
            if (payout.type ==
                ProductCatalogPayout.ProductCatalogPayoutType.Currency)
            {
                if (!Enum.TryParse(payout.subtype, out CoinType coinType))
                {
                    coinType = CoinType.COIN; // fallback
                }

                CoinManager.Instance.IncreaseCurrencyAmount(
                    coinType,
                    (int)payout.quantity
                );
            }
        }
    }


    // --- HELPERS ---

    Product GetFirstProduct(Order order)
    {
        return order.CartOrdered.Items()
            .FirstOrDefault()?.Product;
    }

    void OnProductsFetched(List<Product> products)
    {
        this.products = products;
        //Debug.Log($"Products fetched successfully: {products.Count}");
    }

    void OnProductsFetchFailed(ProductFetchFailed failure)
    {
        Debug.LogError(
            $"Product fetch failed: {failure.FailureReason} " +
            $"({failure.FailedFetchProducts.Count} products)"
        );
    }

    void OnStoreDisconnected(StoreConnectionFailureDescription description)
    {
        Debug.LogError($"Store disconnected: {description.message}");
    }
}

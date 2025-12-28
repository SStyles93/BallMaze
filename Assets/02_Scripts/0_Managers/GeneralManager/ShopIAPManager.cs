using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

public class ShopIAPManager : MonoBehaviour
{
    StoreController m_StoreController;


    void Awake()
    {
        InitializeIAP();
    }

    async void InitializeIAP()
    {
        m_StoreController = UnityIAPServices.StoreController();

        m_StoreController.OnPurchasePending += OnPurchasePending;
        m_StoreController.OnPurchaseConfirmed += OnPurchaseConfirmed;
        m_StoreController.OnPurchaseFailed += OnPurchaseFailed;

        m_StoreController.OnStoreDisconnected += OnStoreDisconnected;
        Debug.Log("Connecting to store.");
        await m_StoreController.Connect();

        m_StoreController.OnProductsFetchFailed += OnProductsFetchedFailed;
        m_StoreController.OnProductsFetched += OnProductsFetched;
        FetchProducts();
    }

    void FetchProducts()
    {
        var initialProductsToFetch = new List<ProductDefinition>
            {
                //new(goldProductId, ProductType.Consumable),
                //new(diamondProductId, ProductType.Consumable)
            };

        m_StoreController.FetchProducts(initialProductsToFetch);
    }
    void OnPurchaseFailed(FailedOrder order)
    {
        var product = GetFirstProductInOrder(order);
        if (product == null)
        {
            Debug.Log("Could not find product in failed order.");
        }

        Debug.Log($"Purchase failed - Product: '{product?.definition.id}'," +
                  $"PurchaseFailureReason: {order.FailureReason.ToString()},"
                  + $"Purchase Failure Details: {order.Details}");
    }

    void OnPurchasePending(PendingOrder order)
    {
        var product = GetFirstProductInOrder(order);
        if (product is null)
        {
            Debug.Log("Could not find product in order.");
            return;
        }

        //Add the purchased product to the players inventory
        //if (product.definition.id == goldProductId)
        //{
        //    AddGold();
        //}
        //else if (product.definition.id == diamondProductId)
        //{
        //    AddDiamond();
        //}

        Debug.Log($"Purchase complete - Product: {product.definition.id}");

        m_StoreController.ConfirmPurchase(order);
    }

    void OnPurchaseConfirmed(Order order)
    {
        switch (order)
        {
            case ConfirmedOrder confirmedOrder:
                OnPurchaseConfirmed(confirmedOrder);
                break;
            case FailedOrder failedOrder:
                OnPurchaseConfirmationFailed(failedOrder);
                break;
            default:
                Debug.Log("Unknown OnPurchaseConfirmed result.");
                break;
        }
    }

    void OnPurchaseConfirmed(ConfirmedOrder order)
    {
        var product = GetFirstProductInOrder(order);
        if (product == null)
        {
            Debug.Log("Could not find product in purchase confirmation.");
        }

        Debug.Log($"Purchase confirmed- Product: {product?.definition.id}");
    }

    void OnPurchaseConfirmationFailed(FailedOrder order)
    {
        var product = GetFirstProductInOrder(order);
        if (product == null)
        {
            Debug.Log("Could not find product in failed confirmation.");
        }

        Debug.Log($"Confirmation failed - Product: '{product?.definition.id}'," +
                  $"PurchaseFailureReason: {order.FailureReason.ToString()},"
                  + $"Confirmation Failure Details: {order.Details}");
    }

    Product GetFirstProductInOrder(Order order)
    {
        return order.CartOrdered.Items().First()?.Product;
    }

    // Calling StoreController.Connect without a listener on the StoreController.OnStoreDisconnected event will result in warnings.
    void OnStoreDisconnected(StoreConnectionFailureDescription description)
    {
        Debug.Log($"Store disconnected details: {description.message}");
    }

    // Calling StoreController.Connect without listeners on StoreController.OnProductsFetched and StoreController.OnProductsFetchedFailed will result in warnings.
    void OnProductsFetched(List<Product> products)
    {
        Debug.Log($"Products fetched successfully for {products.Count} products.");
    }

    void OnProductsFetchedFailed(ProductFetchFailed failure)
    {
        Debug.Log($"Products fetch failed for {failure.FailedFetchProducts.Count} products: {failure.FailureReason}");
    }
}

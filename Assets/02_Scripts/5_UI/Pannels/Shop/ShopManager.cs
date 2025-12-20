using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Shop objects")]
    [SerializeField] private ShopData_SO shopData;
    [SerializeField] private GameObject shopSlotPrefab;
    [Header("Scene References")]
    [SerializeField] private GameObject shopSlotsContainer;

    public static ShopManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        { 
            Destroy(gameObject); 
            return; 
        }
        
        Instance = this;
    }

    private void Start()
    {
        SavingManager.Instance.LoadSession();

        for (int i = 0; i < shopData.shopOptions.Length; i++)
        {
            GameObject shopSlot = Instantiate(shopSlotPrefab, shopSlotsContainer.transform);
            shopSlot.GetComponent<ShopSlot>().InitializeSlot(shopData.shopOptions[i], this);
        }
    }

    public void ProcessShopOption(ShopOption shopOption)
    {
        //TODO: Process the aquired shopOption
        Debug.Log($"ShopOption {shopOption.coinAmountPairs[0].Amount} {shopOption.coinAmountPairs[0].CoinType} " +
            $"for {shopOption.price.value} {shopOption.price.currencyType} is being processed.");
    }
}

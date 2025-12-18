using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Shop objects")]
    [SerializeField] private ShopData_SO shopData;
    [SerializeField] private GameObject shopSlotPrefab;
    [Header("Scene References")]
    [SerializeField] private GameObject shopSlotsHolder;

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



}

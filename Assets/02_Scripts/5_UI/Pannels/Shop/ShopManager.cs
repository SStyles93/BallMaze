using UnityEngine;

public class ShopManager : MonoBehaviour
{
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

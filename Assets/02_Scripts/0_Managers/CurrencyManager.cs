using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] public int currencyValue = 0;

    #region Singleton
    public static CurrencyManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

    }
    #endregion
}

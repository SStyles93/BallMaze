using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] private int currencyValue = 0;

    public event Action<int> OnCurrencyChanged;

    #region Singleton
    public static CurrencyManager Instance { get; private set; }
    public int CurrencyValue { get => currencyValue; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }
    #endregion

    public void IncreaseCurrency(int amount)
    {
        currencyValue += amount;
        OnCurrencyChanged?.Invoke(currencyValue);
    }
    public void SetCurrencyValue(int value)
    {
        currencyValue = value;
        OnCurrencyChanged?.Invoke(currencyValue);
    }

    public void ReduceCurrency(int amount)
    {
        currencyValue -= amount;
        OnCurrencyChanged?.Invoke(currencyValue);
    }

}

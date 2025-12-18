using System;
using System.Collections.Generic;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    private Dictionary<CoinType, int> currencies = new Dictionary<CoinType, int>();
    public int PreviousCoinAmount;
    [SerializeField] int initialHeartAmount = 12;

    /// <summary>
    /// Delegate (Action) used to notify the different pannels (LifePannel, CurrencyPannel, StarPannel)
    /// </summary>
    public event Action<CoinType, int> OnCurrencyChanged;

    public int CoinAmount => currencies[CoinType.COIN];
    public int StarAmount => currencies[CoinType.STAR];
    public int HeartAmount => currencies[CoinType.HEART];
    public int InitialHeartAmount => initialHeartAmount;
    
    public static CoinManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }


    /// <summary>
    /// Checks if the currency amount of a given type in the currency manager is sufficient for a purchase
    /// </summary>
    /// <param name="type">type of the currency</param>
    /// <param name="amount">amount of the currency type</param>
    /// <returns>True if there is enough of the currency type</returns>
    public bool CanAfford(CoinType type, int amount)
    {
        return currencies[type] >= amount;
    }

    /// <summary>
    /// Increases a currency amount of a type in the CurrencyManager
    /// </summary>
    /// <param name="type">Type of currency to increase</param>
    /// <param name="amount">amount to increase by</param>
    public void IncreaseCurrencyAmount(CoinType type, int amount)
    {
        if(type == CoinType.COIN) 
            PreviousCoinAmount = currencies[CoinType.COIN];
        currencies[type] += amount;
        OnCurrencyChanged?.Invoke(type, currencies[type]);
    }

    /// <summary>
    /// Reduce a currency amount of a type in the CurrencyManager
    /// </summary>
    /// <param name="type">Type of currency to reduce</param>
    /// <param name="amount">amount to reduce by</param>
    public void ReduceCurrencyAmount(CoinType type, int amount)
    {
        if (type == CoinType.COIN)
            PreviousCoinAmount = currencies[CoinType.COIN];
        currencies[type] -= amount;
        OnCurrencyChanged?.Invoke(type, currencies[type]);
    }

    /// <summary>
    /// Sets a currency value of a type in the CurrencyManager
    /// </summary>
    /// <param name="type">Type of currency to set</param>
    /// <param name="value">final value of the given currency type held by the currency manager</param>
    public void SetCurrencyAmount(CoinType type, int value)
    {
        if (type == CoinType.COIN)
            PreviousCoinAmount = currencies[CoinType.COIN];
        currencies[type] = value;
        OnCurrencyChanged?.Invoke(type, currencies[type]);
    }

    /// <summary>
    /// Sets the previous currency to be the actual one (Used in text animations)
    /// </summary>
    public void UpdatePreviousCoinAmount()
    {
        PreviousCoinAmount = currencies[CoinType.COIN];
    }
}

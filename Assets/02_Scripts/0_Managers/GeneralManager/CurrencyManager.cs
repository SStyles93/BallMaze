using System;
using UnityEngine;
public enum CurrencyType { COIN, STAR, HEART }

public class CurrencyManager : MonoBehaviour
{

    public int CoinAmount { get => coinAmount; }
    [SerializeField] private int coinAmount = 0;
    public int PreviousCoinAmount;

    public int StarAmount => starAmount;
    [SerializeField] private int starAmount = 0;

    public int HeartAmount => heartAmount;
    [SerializeField] int heartAmount = 3;
    public int InitialHeartAmount => initialHeartAmount;
    [SerializeField] int initialHeartAmount = 12;

    /// <summary>
    /// Delegate called to pass the new amount of coins
    /// </summary>
    /// <remarks>Used by the CurrencyPannel</remarks>
    public event Action<int> OnCoinAmountChanged;
    /// <summary>
    /// Delegate called to pass the new amount of stars
    /// </summary>
    /// <remarks>Used by the StarPannel</remarks>
    public event Action<int> OnStarAmountChanged;
    /// <summary>
    /// Delegate called to pass the new amount of hearths
    /// </summary>
    /// <remarks>Used by the HeartPannel</remarks>
    public event Action<int> OnHeartAmountChanged;

    #region Singleton
    public static CurrencyManager Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }
    #endregion


    /// <summary>
    /// Increases the Coin amount in the CurrencyManager
    /// </summary>
    /// <param name="amount">Amount to Add to the CurrencyManager</param>
    public void IncreaseCurrencyAmount(CurrencyType type, int amount)
    {
        switch (type)
        {
            case CurrencyType.COIN:
                PreviousCoinAmount = coinAmount;
                IncreaseCurrency(amount, ref coinAmount, OnCoinAmountChanged);
                break;
            case CurrencyType.STAR:
                IncreaseCurrency(amount, ref starAmount, OnStarAmountChanged);
                break;
            case CurrencyType.HEART:
                IncreaseCurrency(amount, ref heartAmount, OnHeartAmountChanged);
                break;
        }
        
    }

    /// <summary>
    /// Reduces the Star amount in the CurrencyManager
    /// </summary>
    /// <param name="amount">Amount to substract to the CurrencyManager</param>
    public void ReduceCurrencyAmount(CurrencyType type, int amount)
    {
        switch (type)
        {
            case CurrencyType.COIN:
                PreviousCoinAmount = coinAmount;
                ReduceCurrency(amount, ref coinAmount, OnCoinAmountChanged);
                break;
            case CurrencyType.STAR:
                ReduceCurrency(amount, ref starAmount, OnStarAmountChanged);
                break;
            case CurrencyType.HEART:
                ReduceCurrency(amount, ref heartAmount, OnHeartAmountChanged);
                break;
        }
    }

    /// <summary>
    /// Sets the coin amount of the currency manager
    /// </summary>
    /// <param name="value">final amount of coins held by the currency manager</param>
    public void SetCurrencyAmount(CurrencyType type, int value)
    {
        switch (type)
        {
            case CurrencyType.COIN:
                SetCurrency(value, ref coinAmount, OnCoinAmountChanged);
                UpdatePreviousCoinAmount();
                break;
            case CurrencyType.STAR:
                SetCurrency(value, ref starAmount, OnStarAmountChanged);
                break;
            case CurrencyType.HEART:
                SetCurrency(value, ref  heartAmount, OnHeartAmountChanged);
                break;
        }
        
    }

    /// <summary>
    /// Sets the previous currency to be the actual one (Used in text animations)
    /// </summary>
    public void UpdatePreviousCoinAmount()
    {
        PreviousCoinAmount = coinAmount;
    }


    // --- HELPER FUNCTIONS ---

    /// <summary>
    /// Increases the value of the passed currency and calls a delegate function to send the update to listeners
    /// </summary>
    /// <param name="amount">value to increase by</param>
    /// <param name="currencyVar">"account" to which the value goes</param>
    /// <param name="call">Action called on change</param>
    private void IncreaseCurrency(int amount, ref int currencyVar, System.Action<int> call)
    {
        currencyVar += amount;
        call?.Invoke(currencyVar);
    }

    /// <summary>
    /// Reduces the value of the passed currency and calls a delegate function to send the update to listeners
    /// </summary>
    /// <param name="amount">value to decrease by</param>
    /// <param name="currencyVar">"account" to which the value goes</param>
    /// <param name="call">Action called on change</param>
    private void ReduceCurrency(int amount, ref int currencyVar, System.Action<int> call)
    {
        currencyVar -= amount;
        call?.Invoke(currencyVar);
    }

    private void SetCurrency(int amount, ref int currencyVar, System.Action<int> call)
    {
        currencyVar = amount;
        call?.Invoke(currencyVar);
    }
}

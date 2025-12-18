using System;
using System.Collections.Generic;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    private Dictionary<CoinType, int> coins = new Dictionary<CoinType, int>();
    public int PreviousCoinAmount;

    [Header("Hearts Parameters")]
    [SerializeField] int initialHeartAmount = 15;
    [Tooltip("Time to regain a heart (in Secondd) 1m = 60")]
    [SerializeField] private float timeToRegainHeart = 600;
    private float currentTime = 0;

    /// <summary>
    /// Delegate (Action) used to notify the different pannels (LifePannel, CurrencyPannel, StarPannel)
    /// </summary>
    public event Action<CoinType, int> OnCoinChanged;

    /// <summary>
    /// Delegate to transmit the current time to HeartPannel
    /// </summary>
    public event Action<float> OnTimerChanged;

    public int CoinAmount => coins[CoinType.COIN];
    public int StarAmount => coins[CoinType.STAR];
    public int HeartAmount => coins[CoinType.HEART];
    public int InitialHeartAmount => initialHeartAmount;
    
    public static CoinManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        coins.Add(CoinType.COIN, 0);
        coins.Add(CoinType.STAR, 0);
        coins.Add(CoinType.HEART, 0);
    }

    private void Update()
    {
        TimeCountDown();
    }

    /// <summary>
    /// Checks if the currency amount of a given type in the currency manager is sufficient for a purchase
    /// </summary>
    /// <param name="type">type of the currency</param>
    /// <param name="amount">amount of the currency type</param>
    /// <returns>True if there is enough of the currency type</returns>
    public bool CanAfford(CoinType type, int amount)
    {
        return coins[type] >= amount;
    }

    /// <summary>
    /// Increases a currency amount of a type in the CurrencyManager
    /// </summary>
    /// <param name="type">Type of currency to increase</param>
    /// <param name="amount">amount to increase by</param>
    public void IncreaseCurrencyAmount(CoinType type, int amount)
    {
        if(type == CoinType.COIN) 
            PreviousCoinAmount = coins[CoinType.COIN];
        coins[type] += amount;
        OnCoinChanged?.Invoke(type, coins[type]);
    }

    /// <summary>
    /// Reduce a currency amount of a type in the CurrencyManager
    /// </summary>
    /// <param name="type">Type of currency to reduce</param>
    /// <param name="amount">amount to reduce by</param>
    public void ReduceCurrencyAmount(CoinType type, int amount)
    {
        if (type == CoinType.COIN)
            PreviousCoinAmount = coins[CoinType.COIN];
                
        coins[type] -= amount;

        if (type == CoinType.HEART)
            // Check if a heart was lost && checks if the timer was not already set
            if (coins[CoinType.HEART] < initialHeartAmount && currentTime <= 0)
            {
                //Sets the current timer
                currentTime = timeToRegainHeart;
            }

        OnCoinChanged?.Invoke(type, coins[type]);
    }

    /// <summary>
    /// Sets a currency value of a type in the CurrencyManager
    /// </summary>
    /// <param name="type">Type of currency to set</param>
    /// <param name="value">final value of the given currency type held by the currency manager</param>
    public void SetCurrencyAmount(CoinType type, int value)
    {
        if (type == CoinType.COIN)
            PreviousCoinAmount = coins[CoinType.COIN];
        coins[type] = value;
        OnCoinChanged?.Invoke(type, coins[type]);
    }

    /// <summary>
    /// Sets the previous currency to be the actual one (Used in text animations)
    /// </summary>
    public void UpdatePreviousCoinAmount()
    {
        PreviousCoinAmount = coins[CoinType.COIN];
    }


    private void TimeCountDown()
    {
        if (currentTime <= 0)
        {

            coins[CoinType.HEART]++;
            OnCoinChanged?.Invoke(CoinType.HEART, coins[CoinType.HEART]);
            currentTime = 0;
            return;
        }
        currentTime -= Time.deltaTime;
    }
}

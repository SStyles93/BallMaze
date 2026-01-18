using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    private Dictionary<CoinType, int> coins = new Dictionary<CoinType, int>();
    private Dictionary<CoinType, int> previousCoins = new Dictionary<CoinType, int>();

    [Header("Hearts Parameters")]
    [SerializeField] int maxHeartAmount = 15;
    [Tooltip("Time to regain a heart (in Secondd) 1m = 60")]
    [SerializeField] private int timeToRegainHeartInMinutes = 10;
    private DateTime lastHeartRefillTime;
    Coroutine timerCoroutine;
    bool isDataLoaded = false;

    /// <summary>
    /// Delegate (Action) used to notify the different pannels (LifePannel, CurrencyPannel, StarPannel)
    /// </summary>
    public event Action<CoinType, int, int> OnCoinChanged;
    public event Action<CoinType, int> OnCoinSet;

    public event Action<TimeSpan> OnHeartTimerTick;

    public int CoinAmount => coins[CoinType.COIN];
    public int StarAmount => coins[CoinType.STAR];
    public int HeartAmount => coins[CoinType.HEART];

    public int PreviousCoinAmount => previousCoins[CoinType.COIN];
    public int PreviousStarAmount => previousCoins[CoinType.STAR];
    public int PreviousHeartAmount => previousCoins[CoinType.HEART];

    public int InitialHeartAmount => maxHeartAmount;
    public DateTime LastHeartRefillTime => lastHeartRefillTime;

    public static CoinManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (coins.Count == 0)
        {
            coins.Add(CoinType.COIN, 0);
            coins.Add(CoinType.STAR, 0);
            coins.Add(CoinType.HEART, 0);
        }

        if (previousCoins.Count == 0)
        {
            previousCoins.Add(CoinType.COIN, 0);
            previousCoins.Add(CoinType.STAR, 0);
            previousCoins.Add(CoinType.HEART, 0);
        }
    }

    private void Update()
    {
        // Update calculations & timer (visuals)
        if (Application.isFocused)
        {
            RecalculateHearts();
            if (coins[CoinType.HEART] < maxHeartAmount)
                StartTimer();
        }
        else
        {
            // Only calculations
            RecalculateHearts();
        }
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
        coins[type] += amount;
        if (type == CoinType.HEART)
        {
            RecalculateHearts();
        }

        OnCoinChanged?.Invoke(type, coins[type], previousCoins[type]);
        LevelPreviousCoinAmount(type);
    }

    /// <summary>
    /// Reduce a currency amount of a type in the CurrencyManager
    /// </summary>
    /// <param name="type">Type of currency to reduce</param>
    /// <param name="amount">amount to reduce by</param>
    public void ReduceCurrencyAmount(CoinType type, int amount)
    {
        coins[type] -= amount;

        if (type == CoinType.HEART)
        {
            if (coins[CoinType.HEART] == maxHeartAmount - 1)
            {
                // Start refill timer ONLY when dropping from max
                lastHeartRefillTime = DateTime.UtcNow;
            }
            RecalculateHearts();
        }

        OnCoinChanged?.Invoke(type, coins[type], previousCoins[type]);
        LevelPreviousCoinAmount(type);
    }

    /// <summary>
    /// Sets a currency value of a type in the CurrencyManager
    /// </summary>
    /// <param name="type">Type of currency to set</param>
    /// <param name="value">final value of the given currency type held by the currency manager</param>
    public void SetCurrencyAmount(CoinType type, int value)
    {
        coins[type] = value;
        OnCoinSet?.Invoke(type, value);
    }

    /// <summary>
    /// Levels the previous amount to the current one
    /// </summary>
    /// <param name="type">Type of coin to level</param>
    public void LevelPreviousCoinAmount(CoinType type)
    {
        previousCoins[type] = coins[type];
    }


    public void SetLastHeartRefillTime(DateTime dateTime)
    {
        lastHeartRefillTime = dateTime;
        isDataLoaded = true;
    }
    /// <summary>
    /// Recalculates the amount of hearts to increase the saved data by
    /// </summary>
    private void RecalculateHearts()
    {
        if (coins[CoinType.HEART] >= maxHeartAmount)
        {
            coins[CoinType.HEART] = maxHeartAmount;
            return;
        }

        DateTime now = DateTime.UtcNow;
        TimeSpan elapsed = now - lastHeartRefillTime;

        int heartsToAdd = (int)(elapsed.TotalMinutes / timeToRegainHeartInMinutes);

        if (heartsToAdd <= 0)
            return;

        int totalHeartsAmount = Mathf.Min(coins[CoinType.HEART] + heartsToAdd, maxHeartAmount);

        SetCurrencyAmount(CoinType.HEART, totalHeartsAmount);

        // Move the refill timestamp forward by the amount actually used
        lastHeartRefillTime = lastHeartRefillTime.AddMinutes(
            heartsToAdd * timeToRegainHeartInMinutes
        );

        if (isDataLoaded)
            SavingManager.Instance?.SavePlayer();
    }
    private void StartTimer()
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        timerCoroutine = StartCoroutine(TimerTickRoutine());
    }
    IEnumerator TimerTickRoutine()
    {
        var wait = new WaitForSecondsRealtime(1f);

        while (coins[CoinType.HEART] < maxHeartAmount)
        {
            TimeSpan remaining = TimeUntilNextHeart();

            OnHeartTimerTick?.Invoke(remaining);

            yield return wait;
        }

        // Final update when full
        OnHeartTimerTick?.Invoke(TimeSpan.Zero);
    }
    public TimeSpan TimeUntilNextHeart()
    {
        // If already full, there is no countdown
        if (coins[CoinType.HEART] >= maxHeartAmount)
            return TimeSpan.Zero;

        DateTime now = DateTime.UtcNow;

        DateTime nextHeartTime = lastHeartRefillTime
            .AddMinutes(timeToRegainHeartInMinutes);

        TimeSpan remaining = nextHeartTime - now;

        // Safety clamp (can happen on resume)
        if (remaining < TimeSpan.Zero)
            return TimeSpan.Zero;

        return remaining;
    }
}

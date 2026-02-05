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
    public bool wasCoinsReceived = false;
    public bool wasRocketReceived = false;
    public bool wasUfoReceived = false;


    private DateTime lastHeartRefillTime;
    Coroutine timerCoroutine;
    bool isDataLoaded = false;
    private double rewardedVideoSafeTime;

    /// <summary>
    /// Delegate (Action) used to notify the different pannels (LifePannel, CurrencyPannel, StarPannel)
    /// </summary>
    public event Action<CoinType, int, int> OnCoinChanged;

    /// <summary>
    /// Delegate use to set the amount of coins
    /// </summary>
    public event Action<CoinType, int> OnCoinSet;

    /// <summary>
    /// Delegate used to Tick
    /// </summary>
    public event Action<TimeSpan> OnHeartTimerTick;

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
            coins.Add(CoinType.ROCKET, 0);
            coins.Add(CoinType.UFO, 0);
        }

        if (previousCoins.Count == 0)
        {
            previousCoins.Add(CoinType.COIN, 0);
            previousCoins.Add(CoinType.STAR, 0);
            previousCoins.Add(CoinType.HEART, 0);
            previousCoins.Add(CoinType.ROCKET, 0);
            previousCoins.Add(CoinType.UFO, 0);
        }
    }

    private void Update()
    {
        if(rewardedVideoSafeTime > 0)
        rewardedVideoSafeTime -= Time.deltaTime;

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
    /// Gets the current <see cref="CoinType"/> amount.
    /// </summary>
    /// <param name="type">The type of coin to get the amount for.</param>
    /// <returns>The <see cref="CoinType"/> amount of of the specified <paramref name="type"/>.</returns>
    public int GetCoinAmount(CoinType type) => coins[type];

    /// <summary>
    /// Gets the previous <see cref="CoinType"/> amount.
    /// </summary>
    /// <param name="type">The type of coin to get the amount for.</param>
    /// <returns>The previous <see cref="CoinType"/> amount of the specified <paramref name="type"/>.</returns>
    public int GetPreviousAmount(CoinType type) => previousCoins[type];

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
    /// Special method used to reward heart and apply a safe time frame
    /// </summary>
    /// <param name="amount"></param>
    public void RewardHearts(int amount)
    {
        if (rewardedVideoSafeTime > 0) return;
        rewardedVideoSafeTime = 20.0f;
        
        coins[CoinType.HEART] += amount;
        RecalculateHearts();

        OnCoinChanged?.Invoke(CoinType.HEART, coins[CoinType.HEART], previousCoins[CoinType.HEART]);
        LevelPreviousCoinAmount(CoinType.HEART);
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

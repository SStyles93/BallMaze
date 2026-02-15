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
    [Tooltip("Time to regain a heart")]
    [SerializeField] private int timeToRegainHeartInMinutes = 10;
    [Tooltip("Time to wait before it is possible to replay a rewarded coin video")]
    [SerializeField] private int hoursBetweenRewardedCoins = 4;

    [HideInInspector]
    public bool WasCoinsReceived = false,
        WasRocketReceived = false,
        WasUfoReceived = false;


    Coroutine timerCoroutine;
    private DateTime lastHeartRefillTime;
    private DateTime lastVideoRewardTime;
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
    /// Delegate used to Tick with the RewardedCoin remaining time
    /// </summary>
    public event Action<TimeSpan> OnCoinTimerTick;

    /// <summary>
    /// Delegate used to Tick
    /// </summary>
    public event Action<TimeSpan> OnHeartTimerTick;

    public int InitialHeartAmount => maxHeartAmount;
    public DateTime LastHeartRefillTime => lastHeartRefillTime;
    public DateTime LastVideoRewardTime => lastVideoRewardTime;

    private double currentMinutesUntilFullHearts = -1;
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
        if (rewardedVideoSafeTime > 0)
            rewardedVideoSafeTime -= Time.deltaTime;

        // Update calculations & timer (visuals)
        if (!Application.isFocused) return;

        StartTimer();
        RecalculateHearts();
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

        SavingManager.Instance.SavePlayer();
    }

    public void RewardCoins(int amount)
    {
        if (rewardedVideoSafeTime > 0) return;
        rewardedVideoSafeTime = 20.0f;

        lastVideoRewardTime = DateTime.UtcNow;

        coins[CoinType.COIN] += amount;
        OnCoinChanged?.Invoke(CoinType.COIN, coins[CoinType.COIN], previousCoins[CoinType.COIN]);
        LevelPreviousCoinAmount(CoinType.COIN);

        SavingManager.Instance.SavePlayer();
    }

    public void RewardRocket(int amount)
    {
        coins[CoinType.ROCKET] += amount;
        OnCoinChanged?.Invoke(CoinType.ROCKET, coins[CoinType.ROCKET], previousCoins[CoinType.ROCKET]);
        LevelPreviousCoinAmount(CoinType.ROCKET);
        SavingManager.Instance.SavePlayer();
    }

    public void RewardUfo(int amount)
    {
        coins[CoinType.UFO] += amount;
        OnCoinChanged?.Invoke(CoinType.UFO, coins[CoinType.UFO], previousCoins[CoinType.UFO]);
        LevelPreviousCoinAmount(CoinType.UFO);
        SavingManager.Instance.SavePlayer();
    }

    public void RewardRocketTemporarly(int amount)
    {
        coins[CoinType.ROCKET] += amount;
        OnCoinChanged?.Invoke(CoinType.ROCKET, coins[CoinType.ROCKET], previousCoins[CoinType.ROCKET]);
        LevelPreviousCoinAmount(CoinType.ROCKET);
    }

    public void RewardUfoTemporarly(int amount)
    {
        coins[CoinType.UFO] += amount;
        OnCoinChanged?.Invoke(CoinType.UFO, coins[CoinType.UFO], previousCoins[CoinType.UFO]);
        LevelPreviousCoinAmount(CoinType.UFO);
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
    }

    public void SetLastCoinVideoTime(DateTime dateTime)
    {
        lastVideoRewardTime = dateTime;
    }


    /// <summary>
    /// Recalculates the amount of hearts to increase the saved data by
    /// </summary>
    private void RecalculateHearts()
    {
        if (coins[CoinType.HEART] >= maxHeartAmount)
        {
            coins[CoinType.HEART] = maxHeartAmount;
            NotificationManager.Instance?.CancelHeartNotification();
            currentMinutesUntilFullHearts = -1;
            return;
        }
        //------------------------
        //--- HEART PUSH NOTIF.---
        //------------------------
        else
        {
            int missingHearts = maxHeartAmount - coins[CoinType.HEART];
            int minutesUnityFullHearts = missingHearts * timeToRegainHeartInMinutes;
            if (currentMinutesUntilFullHearts != minutesUnityFullHearts)
            {
                currentMinutesUntilFullHearts = minutesUnityFullHearts;
                DateTime fullHeartsTime = DateTime.UtcNow.AddMinutes(currentMinutesUntilFullHearts);

                Debug.Log($"Scheduling heart notification for {fullHeartsTime} (in {currentMinutesUntilFullHearts} minutes)");
                NotificationManager.Instance?.ScheduleHeartNotification(fullHeartsTime);
            }
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

        TimeSpan remainingHeartTime = TimeUntilNextHeart();
        OnHeartTimerTick?.Invoke(remainingHeartTime);
        TimeSpan remainingCoinVideoTime = TimeUntilNextCoinVideo();
        OnCoinTimerTick?.Invoke(remainingCoinVideoTime);

        yield return wait;
    }

    public TimeSpan TimeUntilNextHeart()
    {
        // If already full, there is no countdown
        if (coins[CoinType.HEART] >= maxHeartAmount)
            return TimeSpan.Zero;

        DateTime now = DateTime.UtcNow;
        DateTime nextHeartTime = lastHeartRefillTime.AddMinutes(timeToRegainHeartInMinutes);

        if (now >= nextHeartTime)
            return TimeSpan.Zero;

        return nextHeartTime - now;
    }

    public TimeSpan TimeUntilNextCoinVideo()
    {
        DateTime now = DateTime.UtcNow;
        DateTime nextPossibleVideo = lastVideoRewardTime.AddHours(hoursBetweenRewardedCoins);

        if (now >= nextPossibleVideo)
            return TimeSpan.Zero;

        return nextPossibleVideo - now;
    }
}

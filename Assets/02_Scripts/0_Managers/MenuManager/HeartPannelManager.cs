using System;
using TMPro;
using Unity.Services.LevelPlay;
using UnityEngine;

public class HeartPannelManager : MonoBehaviour
{
    [SerializeField] int heartValue = 150;
    [SerializeField] private TMP_Text heartTimerText;
    [SerializeField] private TMP_Text heartAmountText;
    [SerializeField] private TMP_Text coinAmountText;

    [SerializeField] GameObject insufficientFundsPannel;

    private void OnEnable()
    {
        CoinManager.Instance.OnCoinSet += SetCurrencyValue;
        CoinManager.Instance.OnCoinChanged += UpdateCurrencyValue;
        CoinManager.Instance.OnHeartTimerTick += UpdateTimerText;
    }

    private void OnDisable()
    {
        CoinManager.Instance.OnCoinSet += SetCurrencyValue;
        CoinManager.Instance.OnCoinChanged -= UpdateCurrencyValue;
        CoinManager.Instance.OnHeartTimerTick -= UpdateTimerText;
    }


    private void Start()
    {
        CoinManager coinManager = CoinManager.Instance;
        if (coinManager != null)
        {
            SetCurrencyValue(CoinType.HEART, coinManager.GetCoinAmount(CoinType.HEART));
        }

        UpdateTimerText(coinManager.TimeUntilNextHeart());
        UpdateCoinValueText();

        AdsManager.Instance.RewardedVideoAd.OnAdRewarded += GrantRewardAndClosePannel;
    }

    private void SetCurrencyValue(CoinType type, int value)
    {
        if (type == CoinType.HEART)
            heartAmountText.text = $"{value.ToString()}";
    }

    private void UpdateCurrencyValue(CoinType type, int value, int previousAmount)
    {
        if (type == CoinType.HEART)
            heartAmountText.AnimateCurrency(previousAmount, value, 1.0f);
    }

    void UpdateTimerText(TimeSpan remaining)
    {
        if (remaining <= TimeSpan.Zero)
        {
            heartTimerText.text = "";
        }
        else
        {
            heartTimerText.text = $"Time to next life : {remaining.Minutes:00}:{remaining.Seconds:00}";
        }
    }

    void UpdateCoinValueText()
    {
        coinAmountText.text = $"<sprite index=0> {heartValue}";
    }


    // --- SceneController ---

    public void ExitHeartPannel()
    {
        SavingManager.Instance.SaveSession();

        // Sends the player to the main menu if the heart pannel is shut (In-Game Mode)
        if (SceneController.Instance.IsGameLoaded())
        {
            SceneController.Instance?.NewTransition()
                .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
                .Unload(SceneDatabase.Scenes.HeartPannel)
                .Unload(SceneDatabase.Scenes.Game)
                .Perform();
        }
        else
        {
            SceneController.Instance?.NewTransition()
                .Unload(SceneDatabase.Scenes.HeartPannel)
                .SetActive(SceneController.Instance.PreviousActiveScene)
                .Perform();
        }
    }

    public void OpenShopMenu()
    {
        SceneController.Instance?.NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.ShopMenu)
            .Unload(SceneDatabase.Scenes.HeartPannel)
            .Unload(SceneDatabase.Scenes.GamesMenu)
            .Perform();
    }

    // --- Purchase ---

    public void ValidateHeartPurchase()
    {
        if (CoinManager.Instance.CanAfford(CoinType.COIN, heartValue))
        {
            CoinManager.Instance.IncreaseCurrencyAmount(CoinType.HEART, 1);
            CoinManager.Instance.ReduceCurrencyAmount(CoinType.COIN, heartValue);
            ExitHeartPannel();
        }
        else
        {
            insufficientFundsPannel.SetActive(true);
            insufficientFundsPannel.GetComponent<InsufficientFundsPannelManager>().InitializePannel(CoinType.COIN);
        }
    }

    // --- Ads ---

    public void LaunchRewardedAd()
    {
        AdsManager manager = AdsManager.Instance;
        if (manager == null) return;

        if (manager.RewardedVideoAd.IsAdReady())
        {
            manager.RewardedVideoAd.ShowAd("rewarded_hearts");
        }
    }

    private void GrantRewardAndClosePannel(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        int rewardAmount = Mathf.Clamp(reward.Amount, 1, 3);
        CoinManager.Instance?.RewardHearts(rewardAmount);

        Debug.Log($"Rewarded :{rewardAmount} {reward.Name}");
        ExitHeartPannel();
    }
}

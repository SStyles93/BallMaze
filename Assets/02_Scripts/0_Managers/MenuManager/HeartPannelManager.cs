using System;
using TMPro;
using Unity.Services.LevelPlay;
using UnityEngine;

public class HeartPannelManager : MonoBehaviour
{
    [SerializeField] int heartValue = 3000;
    [SerializeField] private TMP_Text heartTimerText;
    [SerializeField] private TMP_Text heartAmountText;
    [SerializeField] private TMP_Text coinAmountText;

    [SerializeField] GameObject insufficientFundsPannel;

    private void OnEnable()
    {
        CoinManager.Instance.OnCoinChanged += UpdateCurrencyValue;
        CoinManager.Instance.OnHeartTimerTick += UpdateTimerText;
    }

    private void OnDisable()
    {
        CoinManager.Instance.OnCoinChanged -= UpdateCurrencyValue;
        CoinManager.Instance.OnHeartTimerTick -= UpdateTimerText;
    }


    private void Start()
    {
        CoinManager coinManager = CoinManager.Instance;
        UpdateCurrencyValue(CoinType.HEART, coinManager.HeartAmount);
        UpdateTimerText(coinManager.TimeUntilNextHeart());

        UpdateCoinValueText();

        AdsManager.Instance.RewardedVideoAd.OnAdRewarded += GrantRewardAndClosePannel;
    }


    private void UpdateCurrencyValue(CoinType type, int value)
    {
        if (type == CoinType.HEART)
            heartAmountText.text = $"{value.ToString()}";
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
        coinAmountText.text = $"{heartValue} <sprite index=0>";
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
        if(CoinManager.Instance.CanAfford(CoinType.COIN, heartValue))
        {
            CoinManager.Instance.IncreaseCurrencyAmount(CoinType.HEART, 3);
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
            manager.RewardedVideoAd.ShowAd();
        }
    }

    private void GrantRewardAndClosePannel(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        Debug.Log($"Rewarded :{reward.Amount} {reward.Name}");
        CoinManager.Instance?.IncreaseCurrencyAmount(CoinType.HEART, reward.Amount);
        ExitHeartPannel();
    }
}

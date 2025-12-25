using UnityEngine;
using Unity.Services.LevelPlay;

public class HeartPannelManager : MonoBehaviour
{
    [SerializeField] int heartValue = 3000;
    [SerializeField] GameObject insufficientFundsPannel;

    private void Start()
    {
        AdsManager.Instance.RewardedVideoAd.OnAdRewarded += GrantRewardAndClosePannel;
    }

    public void ExitHeartPannel()
    {
        SavingManager.Instance.SaveSession();

        SceneController.Instance?.NewTransition()
            .Unload(SceneDatabase.Scenes.HeartPannel)
            .Perform();
    }

    public void OpenShopMenu()
    {
        SceneController.Instance?.NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.ShopMenu)
            .Unload(SceneDatabase.Scenes.HeartPannel)
            .Unload(SceneDatabase.Scenes.GamesMenu)
            .Perform();
    }

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

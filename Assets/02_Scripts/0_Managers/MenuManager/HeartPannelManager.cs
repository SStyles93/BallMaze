using UnityEngine;

public class HeartPannelManager : MonoBehaviour
{
    [SerializeField] int heartValue = 3000;
    [SerializeField] GameObject insufficientFundsPannel;

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
}

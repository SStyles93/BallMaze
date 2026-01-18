using TMPro;
using UnityEngine;

public class ContinuePannelManager : MonoBehaviour
{
    [SerializeField] private TMP_Text continueText;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private TMP_Text restartText;

    [Space(10)]
    [SerializeField] int heartValueInCoins = 300;

    private void Start()
    {
        continueText.text = $"<sprite index=0>{heartValueInCoins} = <sprite index=1> + 1";

        bool canRestart = CoinManager.Instance.HeartAmount > 0;
        restartButton.SetActive(canRestart);

        if (canRestart)
        {
            int heartsLeft = Mathf.Clamp(CoinManager.Instance.HeartAmount, 1, 3);
            restartText.text = $"Restart <sprite index=1> - {heartsLeft}";
        }
    }


    public void Continue()
    {
        CoinManager.Instance.IncreaseCurrencyAmount(CoinType.HEART, 1);
        CoinManager.Instance.ReduceCurrencyAmount(CoinType.COIN, heartValueInCoins);
        LifeManager.Instance.SetLife(1);
        this.gameObject.SetActive(false);
    }

    public void ReturnToGamesMenu()
    {
        LifeManager.Instance.ResetLife();
        LevelManager.Instance.MarkLevelAsFailed();

        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
            .Unload(SceneDatabase.Scenes.Game)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }

    public void Restart()
    {
        LifeManager.Instance.ResetLife();

        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.Game)
            .Unload(SceneDatabase.Scenes.Game)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }
}

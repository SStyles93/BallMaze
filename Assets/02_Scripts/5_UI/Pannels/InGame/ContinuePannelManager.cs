using System;
using TMPro;
using UnityEngine;

public class ContinuePannelManager : MonoBehaviour
{
    [SerializeField] private TMP_Text levelText;
    
    [Header("Buttons")]
    [SerializeField] private GameObject continueButton;
    [SerializeField] private TMP_Text continueText;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private TMP_Text restartText;
    [SerializeField] private GameObject exitButton;

    [Space(10)]
    [SerializeField] int heartValueInCoins = 300;

    private int numberOfTrials = 1;

    private void OnEnable()
    {
        levelText.text = $"Level {LevelManager.Instance.CurrentLevelIndex}";

        if (CoinManager.Instance.CanAfford(CoinType.COIN, heartValueInCoins * numberOfTrials))
        {
            continueButton.SetActive(true);
            continueText.text = $"<sprite index=0> {heartValueInCoins * numberOfTrials} <sprite index=2> + 1";
        }
        else
        {
            continueButton.SetActive(false);
            exitButton.SetActive(true);
        }


            bool canRestart = CoinManager.Instance.GetCoinAmount(CoinType.HEART) > 0;
        restartButton.SetActive(canRestart);

        if (canRestart)
        {
            int heartsLeft = Mathf.Clamp(CoinManager.Instance.GetCoinAmount(CoinType.HEART), 1, 3);
            restartText.text = $"Restart <sprite index=2> - {heartsLeft}";
        }
    }


    public void Continue()
    {
        if (CoinManager.Instance.CanAfford(CoinType.COIN, heartValueInCoins * numberOfTrials))
        {
            CoinManager.Instance.IncreaseCurrencyAmount(CoinType.HEART, 1);
            CoinManager.Instance.ReduceCurrencyAmount(CoinType.COIN, heartValueInCoins);

            LifeManager.Instance.SetLife(1);

            GameStateManager.Instance?.SetState(GameState.Playing);

            numberOfTrials++;

            this.gameObject.SetActive(false);
        }
    }

    public void ReturnToGamesMenu()
    {
        LifeManager.Instance.ResetLife();
        
        LevelManager.Instance.MarkLevelAsFailed();

        SavingManager.Instance.SaveSession();
        
        GameStateManager.Instance?.SetState(GameState.Playing);

        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
            .Unload(SceneController.Instance.CurrentActiveScene)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }

    public void Restart()
    {
        LifeManager.Instance.ResetLife();
        
        LevelManager.Instance.CurrentStarCount = 0;

        GameStateManager.Instance?.SetState(GameState.Playing);

        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Content, SceneController.Instance.CurrentActiveScene)
            .Unload(SceneController.Instance.CurrentActiveScene)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }
}

using System;
using UnityEngine;

public class LifeManager : MonoBehaviour
{
    public int CurrentLife => currentLife;
    [SerializeField] int currentLife = 3;

    /// <summary>
    /// Delegate used in the life pannel (In Game)
    /// </summary>
    public event Action OnRemoveLife;
    public event Action OnLifeRegained;

    #region Singleton
    public static LifeManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }
    #endregion

    private void Start()
    {
        ResetLife();
    }

    public void RemoveLife()
    {
        if(currentLife > 0)
        {
            currentLife--;
            CoinManager.Instance.ReduceCurrencyAmount(CoinType.HEART, 1);
        }

        LevelManager.Instance.IncreaseLivesLostToThisLevel();
        OnRemoveLife?.Invoke();

        //if (currentLife <= 0)
        //    KillPlayer();
    }

    /// <summary>
    /// Sets the amount of life according to the Heart currency amount
    /// </summary>
    public void ResetLife()
    {
        CoinManager currencyManager = CoinManager.Instance;
        // Sets the amount of life according to the Heart currency amount
        if (currencyManager.HeartAmount >= 3)
            //(max 3 hearts in game)
            currentLife = 3;
        else
            currentLife = currencyManager.HeartAmount;
    }

    public void SetLife(int value)
    {
        currentLife = value;
        // Used by the dead zone to resume game
        OnLifeRegained?.Invoke();
    }


    private void KillPlayer()
    {
        LevelManager levelManager = LevelManager.Instance;

        // Remove Level Data from saving
        if (levelManager.PreviousNumberOfStars == 0 && levelManager.WasGamePreviouslyFinished == false)
            LevelManager.Instance.MarkLevelAsFailed();

        // Save Session
        SavingManager.Instance.SaveSession();

        // Opens the Heart pannel if the player has no more hearts
        //if(!CoinManager.Instance.CanAfford(CoinType.HEART, 1))
        //{
        //    SceneController.Instance.NewTransition()
        //        .Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.HeartPannel)
        //        .Perform();
        //}
        //else
        //{
        //    // Open EndPannel
        //    SceneController.Instance.NewTransition()
        //        .Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.EndPannel)
        //        .Perform();
        //}
    }
}

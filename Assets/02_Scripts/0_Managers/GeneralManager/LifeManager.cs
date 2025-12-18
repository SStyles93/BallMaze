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

        OnRemoveLife?.Invoke();

        if (currentLife <= 0)
            KillPlayer();
    }

    /// <summary>
    /// // Sets the amount of life according to the Heart currency amount
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

    private void KillPlayer()
    {
        LevelManager levelManager = LevelManager.Instance;

        // Remove Level Data from saving
        if (levelManager.PreviousNumberOfStars == 0 && levelManager.WasGamePreviouslyFinished == false)
            LevelManager.Instance.RemoveCurrentLevelData();

        // Save Session
        SavingManager.Instance.SaveSession();

        // Open EndPannel
        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.EndPannel)
            .Perform();
    }
}

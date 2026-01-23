using System;
using UnityEngine;

public class LifeManager : MonoBehaviour
{
    public int CurrentLife => currentLife;
    [SerializeField] int currentLife = 3;

    /// <summary>
    /// Delegate used in the life pannel (In Game)
    /// </summary>
    public event Action OnLifeRemoved;
    public event Action OnLifeIncreased;

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
        OnLifeRemoved?.Invoke();

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
        OnLifeIncreased?.Invoke();
        currentLife = value;
    }
}

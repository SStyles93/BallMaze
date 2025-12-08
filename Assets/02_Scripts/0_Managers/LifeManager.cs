using System;
using UnityEngine;

public class LifeManager : MonoBehaviour
{
    public static LifeManager Instance { get; private set; }

    public int CurrentLife { get => currentLife; set => currentLife = value; }
    [SerializeField] int currentLife = 3;

    public event Action OnRemoveLife;

    #region Singleton

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }
    #endregion

    public void RemoveLife()
    {
        if(currentLife > 0)
        {
            currentLife--;
        }

        OnRemoveLife?.Invoke();

        if (currentLife <= 0)
            KillPlayer();
    }

    public void ResetLife()
    {
        currentLife = 3;
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

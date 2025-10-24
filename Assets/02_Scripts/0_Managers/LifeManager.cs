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
    }

    public void ResetLife()
    {
        currentLife = 3;
    }
}

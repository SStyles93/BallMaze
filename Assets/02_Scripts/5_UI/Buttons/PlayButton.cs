using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PlayButton : UIButton
{
    [SerializeField] private TMP_Text m_levelIndexText;

    [Header("Animation")]
    [SerializeField] public float scaleUp = 1.15f;
    [SerializeField] public float duration = 0.15f;

    private int m_indexOfLevelToPlay = 0;
    Vector3 initialScale;

    private void Awake()
    {
        initialScale = transform.localScale;
    }

    protected override void Start()
    {
        base.Start();
        //Get index of current level
        button.onClick.AddListener(PlayNextLevel);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        button.onClick.RemoveListener(PlayNextLevel);
    }

    public void InitializeLastLevelToPlay()
    {
        m_indexOfLevelToPlay = LevelManager.Instance.GetHighestFinishedLevelIndex()+1;
        m_levelIndexText.text = m_indexOfLevelToPlay.ToString();
    }

    public void SetIndexOfLevelToPlay(int index)
    {
        m_indexOfLevelToPlay = index;
        m_levelIndexText.text = index.ToString();
        // Scale
        PlayHighlight();

    }

    private void PlayNextLevel()
    {
        GamesMenuManager.Instance?.SaveScrollbarValues();
        SavingManager.Instance?.SaveSession();

        // Normal behaviour
        if (CoinManager.Instance.CanAfford(CoinType.HEART, 1))
        {
            // Level all the currencies (enables animation afterwards)
            CoinManager.Instance.LevelPreviousCoinAmount(CoinType.COIN);
            CoinManager.Instance.LevelPreviousCoinAmount(CoinType.STAR);
            CoinManager.Instance.LevelPreviousCoinAmount(CoinType.HEART);

            LevelManager.Instance.InitializeLevel(m_indexOfLevelToPlay);

            if (Enum.TryParse<SceneDatabase.Scenes>(
                SceneManager.GetActiveScene().name, out SceneDatabase.Scenes scene))
            {

                SceneController.Instance
                .NewTransition()
                .Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.Game)
                .Unload(scene)
                .WithOverlay()
                .Perform();
            }
        }
        else // Heart Pannel
        {
            SceneController.Instance
                .NewTransition()
                .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.HeartPannel)
                .Perform();
        }

    }

    private void PlayHighlight()
    {
        transform
            .DOScale(initialScale * scaleUp, duration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                transform.DOScale(initialScale, duration)
                         .SetEase(Ease.InBack);
            });
    }
}

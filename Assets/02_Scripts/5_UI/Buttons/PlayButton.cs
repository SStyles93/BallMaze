using System;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;

public class PlayButton : UIButton
{
    [SerializeField] private TMP_Text m_levelIndexText;
    private int m_indexOfLevelToPlay = 0;

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
        m_indexOfLevelToPlay = LevelManager.Instance.GetLastLevelIndex() + 1;
        m_levelIndexText.text = (m_indexOfLevelToPlay).ToString();
    }

    public void SetIndexOfLevelToPlay(int index)
    {
        m_indexOfLevelToPlay = index;
        m_levelIndexText.text = (m_indexOfLevelToPlay).ToString();
    }

    private void PlayNextLevel()
    {
        GamesMenuManager.Instance?.SaveScrollbarValues();

        // Normal behaviour
        if (CoinManager.Instance.CanAfford(CoinType.HEART, 1))
        {
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
}

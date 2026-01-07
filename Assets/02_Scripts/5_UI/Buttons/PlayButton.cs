using TMPro;
using UnityEngine;

public class PlayButton : UIButton
{
    [SerializeField] private TMP_Text m_levelIndexText;

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

    public void InitializeNextLevelText()
    {
        m_levelIndexText.text = (LevelManager.Instance.GetLastLevelIndex() + 1).ToString();
    }

    private void PlayNextLevel()
    {
        GamesMenuManager.Instance?.SaveScrollbarValues();

        // Normal behaviour
        if (CoinManager.Instance.CanAfford(CoinType.HEART, 1))
        {
            LevelManager.Instance.InitializeLevel(LevelManager.Instance.GetLastLevelIndex() + 1);
            SceneController.Instance
                .NewTransition()
                .Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.Game)
                .Unload(SceneDatabase.Scenes.GamesMenu)
                .WithOverlay()
                .Perform();
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

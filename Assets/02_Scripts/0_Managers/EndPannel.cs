using System.Collections.Generic;   
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndPannel : MonoBehaviour
{
    [Header("Object Reference")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private List<Image> starImages = new List<Image>();
    [SerializeField] private TMP_Text scoreText;

    [Header("End Pannel Assets")]
    [SerializeField] private Sprite starSprite;

    private LevelManager levelManager;

    private void Start()
    {
        levelManager = LevelManager.Instance;

        levelText.text = $"Level {levelManager.CurrentLevelIndex}";
        for (int i = 0; i < levelManager.CurrentLevelData.levelGrade; i++) 
        {
            starImages[i].sprite = starSprite;
        }
        scoreText.text = "???"; //TODO: max score - time & lost life
        //scoreText.text = levelManager.CurrentLevelData.levelTime.ToString();
    }


    public void LoadNextScene()
    {
        levelManager.InitializeLevel(levelManager.CurrentLevelIndex + 1);

        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.Game)
            .Unload(SceneDatabase.Scenes.Game)
            .Unload(SceneDatabase.Scenes.EndPannel)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }

    public void ReturnToGamesMenu()
    {
        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
            .Unload(SceneDatabase.Scenes.Game)
            .Unload(SceneDatabase.Scenes.EndPannel)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }
}

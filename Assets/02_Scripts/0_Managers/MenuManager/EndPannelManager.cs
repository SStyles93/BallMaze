using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndPannelManager : MonoBehaviour
{
    [Header("Object Reference")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private GameObject continueButton;

    private LevelManager levelManager;

    private void Start()
    {
        levelManager = LevelManager.Instance;

        // LVL n°
        levelText.text = $"Level {levelManager.CurrentLevelIndex}";

        continueButton.SetActive(true);
        // START ( * * * )
        if (levelManager.CurrentLevelData.numberOfStars >= 3)
            AudioManager.Instance?.PlayWinSound();
    }

    /// <summary>
    /// Loads the current scene (RETRY)
    /// </summary>
    public void LoadCurrentScene()
    {
            LoadScene(0);
    }

    /// <summary>
    /// Loads the next scene (CONTINUE)
    /// </summary>
    public void LoadNextScene()
    {
        LoadScene(1);
    }

    /// <summary>
    /// Loading (LevelManager initialize, Life reset, Scene transition)
    /// </summary>
    /// <param name="sceneIndex">0 = current, 1 = next</param>
    private void LoadScene(int sceneIndex)
    {
        levelManager.InitializeLevel(levelManager.CurrentLevelIndex + sceneIndex);

        LifeManager.Instance.SetLife();

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
        LifeManager.Instance.SetLife();

        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
            .Unload(SceneDatabase.Scenes.Game)
            .Unload(SceneDatabase.Scenes.EndPannel)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }


}

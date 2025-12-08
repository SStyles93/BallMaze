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
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject retryButton;

    private LevelManager levelManager;

    private void Start()
    {
        levelManager = LevelManager.Instance;

        // lvl n°
        levelText.text = $"Level {levelManager.CurrentLevelIndex}";

        //stars ( * * * )
        int numberOfStars = levelManager.CurrentLevelData.numberOfStars;
        for (int i = 0; i < numberOfStars; i++) 
        {
            starImages[i].enabled = true;
        }

        if (numberOfStars == 0 || LifeManager.Instance.CurrentLife == 0)
        {
            //Loads the current scene
            retryButton.SetActive(true);
            continueButton.SetActive(false);
        }
        else
        {
            //Loads the next scene
            continueButton.SetActive(true);
            retryButton.SetActive(false);
        }
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

        LifeManager.Instance.ResetLife();

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
        LifeManager.Instance.ResetLife();

        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
            .Unload(SceneDatabase.Scenes.Game)
            .Unload(SceneDatabase.Scenes.EndPannel)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }
}

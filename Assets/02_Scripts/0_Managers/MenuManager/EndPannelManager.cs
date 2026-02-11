using TMPro;
using UnityEngine;

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
        {
            AudioManager.Instance?.PlayWinSound();
            GoogleReviewManager.Instance.RequestReview();
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
        LifeManager.Instance.ResetLife();

        levelManager.InitializeLevel(levelManager.CurrentLevelIndex + sceneIndex);
        
        SceneController.SceneTransitionPlan customPlan  = SceneController.Instance.NewTransition();
        customPlan.Unload(SceneController.Instance.PreviousActiveScene);
        levelManager.LoadLevel(levelManager.CurrentLevelIndex, customPlan);
    }

    public void ReturnToGamesMenu()
    {
        LifeManager.Instance.ResetLife();

        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
            .Unload(SceneController.Instance.PreviousActiveScene)
            .Unload(SceneDatabase.Scenes.EndPannel)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }


}

using UnityEngine;

public class CustomizationMenuManager : MonoBehaviour
{
    [SerializeField] private PlayButton playButton;

    private void Start()
    {
        SavingManager.Instance.LoadSession();
        playButton.InitializeLastLevelToPlay();
    }

    public void OpenGamesMenu()
    {
        SaveSession();

        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
            .Unload(SceneDatabase.Scenes.CustomizationMenu)
            .WithOverlay()
            .Perform();
    }
    public void OpenShopMenu()
    {
        SaveSession();

        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.ShopMenu)
            .Unload(SceneDatabase.Scenes.CustomizationMenu)
            .WithOverlay()
            .Perform();
    }

    public void SaveSession()
    {
        SavingManager.Instance.SaveSession();
    }
}

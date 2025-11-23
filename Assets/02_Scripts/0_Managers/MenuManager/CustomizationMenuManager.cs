using UnityEngine;

public class CustomizationMenuManager : MonoBehaviour
{
    public void OpenGamesMenu()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
            .Unload(SceneDatabase.Scenes.CustomizationMenu)
            .WithOverlay()
            .Perform();
    }
    public void OpenMainMenu()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu)
            .Unload(SceneDatabase.Scenes.CustomizationMenu)
            .WithOverlay()
            .Perform();
    }
}

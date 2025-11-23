using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public void OpenGamesMenu()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
            .Unload(SceneDatabase.Scenes.MainMenu)
            .WithOverlay()
            .Perform();
    }

    public void OpenCustomizationMenu()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.CustomizationMenu)
            .Unload(SceneDatabase.Scenes.MainMenu)
            .WithOverlay()
            .Perform();
    }

    public void OpenSettingsMenu()
    {
        //SceneController.Instance
        //    .NewTransition()
        //    .Load(SceneDatabase.Slots.Session, SceneDatabase.Scenes.Session.ToString())
        //    .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.SavedGamesMenu.ToString())
        //    .Unload(SceneDatabase.Scenes.MainMenu.ToString())
        //    .Perform();
    }

}

using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public void OpenSavedGamesMenu()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.Null)
            .Unload(SceneDatabase.Scenes.MainMenu)
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

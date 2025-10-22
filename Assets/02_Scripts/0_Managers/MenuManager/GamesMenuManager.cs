using UnityEngine;

public class GamesMenuManager : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu)
            .Unload(SceneDatabase.Scenes.GamesMenu)
            .WithOverlay()
            .Perform();

    }
}

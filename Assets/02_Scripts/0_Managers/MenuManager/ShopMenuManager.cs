using UnityEngine;

public class ShopMenuManager : MonoBehaviour
{
    public void OpenGamesMenu()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
            .Unload(SceneDatabase.Scenes.ShopMenu)
            .WithOverlay()
            .Perform();
    }
}

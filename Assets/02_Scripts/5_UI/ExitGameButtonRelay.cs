using UnityEngine;

public class ExitGameButtonRelay : MonoBehaviour
{
    public void ExitGame()
    {
        SceneController.Instance.NewTransition()
            .Unload(SceneDatabase.Scenes.Game)
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
            .WithOverlay()
            .Perform();
    }
}

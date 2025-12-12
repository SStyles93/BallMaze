using UnityEngine;

public class ExitGameButtonRelay : UIButton
{
    public void ExitGame()
    {
        LifeManager.Instance.ResetLife();

        SceneController.Instance.NewTransition()
            .Unload(SceneDatabase.Scenes.Game)
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
            .WithOverlay()
            .Perform();
    }
}

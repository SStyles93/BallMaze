using UnityEngine;

public class HomeButton : UIButton
{
    public void ExitGame()
    {
        LifeManager.Instance?.SetLife();

        SavingManager.Instance?.SavePlayer();

        SceneController.Instance.NewTransition()
            .Unload(SceneDatabase.Scenes.Game)
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
            .WithOverlay()
            .Perform();
    }
}

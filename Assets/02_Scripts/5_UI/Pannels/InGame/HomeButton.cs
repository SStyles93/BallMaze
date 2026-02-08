using UnityEngine;

public class HomeButton : UIButton
{
    public void ExitGame()
    {
        LifeManager.Instance?.ResetLife();

        SavingManager.Instance?.SavePlayer();

        SceneController.Instance.NewTransition()
            .Unload(SceneController.Instance.CurrentActiveScene)
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
            .WithOverlay()
            .Perform();
    }
}

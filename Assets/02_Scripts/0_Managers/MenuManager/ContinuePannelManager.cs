using UnityEngine;

public class ContinuePannelManager : MonoBehaviour
{
    private void Start()
    {
        // Check life left -> 0 disactivate restart -> Active Exit
    }

    public void ReturnToGamesMenu()
    {
        LifeManager.Instance.SetLife();

        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
            .Unload(SceneDatabase.Scenes.Game)
            .Unload(SceneDatabase.Scenes.ContinuePannel)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }

    public void Restart()
    {
        LifeManager.Instance.SetLife();

        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.Game)
            .Unload(SceneDatabase.Scenes.Game)
            .Unload(SceneDatabase.Scenes.ContinuePannel)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }
}

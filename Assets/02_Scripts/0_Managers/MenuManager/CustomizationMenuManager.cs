using UnityEngine;

public class CustomizationMenuManager : MonoBehaviour
{
    private void Start()
    {
        SavingManager.Instance.LoadSession();
    }

    public void OpenGamesMenu()
    {
        SaveSession();

        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
            .Unload(SceneDatabase.Scenes.CustomizationMenu)
            .WithOverlay()
            .Perform();
    }
    public void OpenShopMenu()
    {
        SaveSession();

        //SceneController.Instance
        //    .NewTransition()
        //    .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu)
        //    .Unload(SceneDatabase.Scenes.CustomizationMenu)
        //    .WithOverlay()
        //    .Perform();
    }

    public void SaveSession()
    {
        SavingManager.Instance.SaveSession();
    }
}

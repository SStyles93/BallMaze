using UnityEngine;

public class CustomizationMenuManager : MonoBehaviour
{
    public static CustomizationMenuManager Instance { get; private set; }

    public CustomizationData_SO customizationData_SO;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }


    public void OpenGamesMenu()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
            .Unload(SceneDatabase.Scenes.CustomizationMenu)
            .WithOverlay()
            .WithSave()
            .Perform();
    }
    public void OpenMainMenu()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu)
            .Unload(SceneDatabase.Scenes.CustomizationMenu)
            .WithOverlay()
            .WithSave()
            .Perform();
    }
}

using UnityEngine;

public class StartMenuManager : MonoBehaviour
{
    [SerializeField] private float startTimer = 2.0f;

    private float currentStartTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentStartTimer = startTimer;
    }

    // Update is called once per frame
    void Update()
    {
        currentStartTimer -= Time.deltaTime;

        if(currentStartTimer <= 0f)
        {
            LoadGameScene();
        }
    }
    private void LoadGameScene()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
            .Unload(SceneDatabase.Scenes.StartMenu)
            .WithOverlay()
            .Perform();
    }
}

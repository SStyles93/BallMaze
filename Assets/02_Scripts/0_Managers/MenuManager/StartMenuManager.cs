using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

public class StartMenuManager : MonoBehaviour
{
    [SerializeField] private float startTimer = 2.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        AudioManager.Instance?.PlayMusic();

        await WaitForStartupSequence();

        LoadGameScene();
    }

    private async Task WaitForStartupSequence()
    {
        Task minTimeTask = Task.Delay((int)(startTimer * 1000));

        Task bootPipelineTask = RunBootPipeline();

        await Task.WhenAll(minTimeTask, bootPipelineTask);
    }

    private async Task RunBootPipeline()
    {
        if (LoginManager.Instance != null)
        {
            // Authentication with timeout
            await Task.WhenAny(
                LoginManager.Instance.AuthenticationTask, Task.Delay(5000) // 3s timeout
            );
        }

        if (CloudSaveManager.Instance != null)
        {
            // Cloud initialization with timeout
            if (CloudSaveManager.Instance.IsAvailable)
            {
                await Task.WhenAny(
                    CloudSaveManager.Instance.InitializationTask, Task.Delay(5000)
                );
            }
        }
    }

    private void LoadGameScene()
    {
        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
            .Unload(SceneDatabase.Scenes.StartMenu)
            .WithOverlay()
            .Perform();
    }

}

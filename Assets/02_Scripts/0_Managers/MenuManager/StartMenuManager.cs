using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

public class StartMenuManager : MonoBehaviour
{
    [SerializeField] private float startTimer = 2.0f;

    private float currentStartTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        currentStartTimer = startTimer;
        AudioManager.Instance?.PlayMusic();

        await WaitForStartupSequence();

        LoadGameScene();
    }

    private async Task WaitForStartupSequence()
    {
        // Task 1 - Minimum display time
        Task minTimeTask = Task.Delay((int)(startTimer * 1000));

        // Task 2 - Run booting pipeline (auth + cloud)
        Task bootPipelineTask = RunBootPipeline();

        // Wait for BOTH to finish (whichever takes longer)
        await Task.WhenAll(minTimeTask, bootPipelineTask);
    }

    private async Task RunBootPipeline()
    {
        if (LoginManager.Instance != null)
        {
            // Authentication with timeout
            await Task.WhenAny(
                LoginManager.Instance.AuthenticationTask, Task.Delay(3000) // 3s timeout
            );
        }

        if (CloudSaveManager.Instance != null)
        {
            // Cloud initialization with timeout
            if (CloudSaveManager.Instance.IsAvailable)
            {
                await Task.WhenAny(
                    CloudSaveManager.Instance.InitializationTask, Task.Delay(3000)
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

using System;
using UnityEngine;

public class StartMenuManager : MonoBehaviour
{
    [SerializeField] private float startTimer = 2.0f;

    private float currentStartTimer = 0f;

    private const string CloudLoadLastDateKey = "CloudLoad_LastDate";
    private readonly TimeSpan CloudLoadInterval = TimeSpan.FromDays(2);


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentStartTimer = startTimer;
        AudioManager.Instance?.PlayMusic();
    }

    // Update is called once per frame
    void Update()
    {
        currentStartTimer -= Time.deltaTime;

        if (currentStartTimer <= 0f)
        {
            LoadGameScene();
        }
    }
    private void LoadGameScene()
    {

        SceneController.SceneTransitionPlan plan = SceneController.Instance.NewTransition();

        plan.Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.GamesMenu)
            .Unload(SceneDatabase.Scenes.StartMenu)
            .WithOverlay();

        if (ShouldLoadFromCloud())
        {
            plan.WithCloudLoad();
            // Update the last cloud load date
            PlayerPrefs.SetString(CloudLoadLastDateKey, DateTime.Now.ToString());
            PlayerPrefs.Save();
        }

        plan.Perform();
    }

    private bool ShouldLoadFromCloud()
    {
        // First install (no key yet)
        if (!PlayerPrefs.HasKey(CloudLoadLastDateKey))
        {
            return true;
        }

        // Parse last cloud load date
        string savedDateStr = PlayerPrefs.GetString(CloudLoadLastDateKey, "");
        if (DateTime.TryParse(savedDateStr, out DateTime lastLoadDate))
        {
            if (DateTime.Now - lastLoadDate >= CloudLoadInterval)
                return true;
            else
                return false;
        }

        // If parse failed, default to load
        return true;
    }

}

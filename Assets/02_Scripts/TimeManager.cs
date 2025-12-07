using UnityEngine;
using TMPro;
using System;

public class TimeManager : MonoBehaviour
{
    [SerializeField] PcgData_SO pcgData;
    public static float currentElapsedTime = 0.0f;
    public static bool IsTimeUpdated = false;

    private int currentLevelIndex = 0;
    private float levelTime = 0f;

    [SerializeField] private TMP_Text timerText;

    private void Start()
    {
        currentElapsedTime = 0.0f;
        currentLevelIndex = LevelManager.Instance.CurrentLevelIndex;
        levelTime = pcgData.levelParameters[currentLevelIndex].timeToComplete;
    }

    // Update is called once per frame
    void Update()
    {
        DefineTextColorAccordingToTime();
        DisplayTime();
    }

    private void DefineTextColorAccordingToTime()
    {
        float yellowThreshold = levelTime * 0.8f;

        if(currentElapsedTime > levelTime) timerText.color = Color.red;
        else if(currentElapsedTime > yellowThreshold) timerText.color = Color.yellow;
        else timerText.color = Color.green;
    }

    private void DisplayTime()
    {
        if (IsTimeUpdated)
        {
            currentElapsedTime += Time.deltaTime;
            timerText.text = TimeSpan.FromSeconds((double)currentElapsedTime).ToString(@"mm\:ss");
        }
    }

    public void SaveTimeForLevel()
    {
        if (pcgData.TIME_DEFINITION_IS_ACTIVE)
        {
            pcgData.levelParameters[LevelManager.Instance.CurrentLevelIndex].timeToComplete = currentElapsedTime;
        }
    }
}

using UnityEngine;
using TMPro;
using Unity.VisualScripting;
public class TimeDefinitionSaver : MonoBehaviour
{
    [SerializeField] PcgData_SO pcgData;
    public static float levelTime = 0.0f;
    public static bool IsTimeUpdated = false;
    private int levelIndex;


    [SerializeField] private TMP_Text timerText;

    private void Start()
    {
        levelIndex = LevelManager.Instance.CurrentLevelIndex;
        levelTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsTimeUpdated)
        {
            levelTime += Time.deltaTime;
            timerText.text = levelTime.ToString() + $"\nLevel: {levelIndex}";
        }
    }

    public void SaveTimeForLevel()
    {
        pcgData.levelParameters[LevelManager.Instance.CurrentLevelIndex].timeToComplete = levelTime;
    }
}

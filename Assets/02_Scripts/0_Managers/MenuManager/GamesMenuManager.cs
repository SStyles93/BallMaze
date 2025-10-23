using Newtonsoft.Json.Serialization;
using UnityEngine;

public class GamesMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject scrollViewContent;
    [SerializeField] private GameObject slotPrefab;

    [SerializeField] private int numberOfLevels = 10;

    private void Start()
    {
        SavingManager.Instance.LoadSession();

        int levelManagerCount = LevelManager.Instance.KvpLevelData.Count;
        if (levelManagerCount % 10 == 0 || levelManagerCount <= 0)
        {
            numberOfLevels = LevelManager.Instance.KvpLevelData.Count + 10;
        }
        InitializeSlots(numberOfLevels);
    }

    private void InitializeSlots(int numberOfLevels)
    {
        int levelManagerCount = LevelManager.Instance.KvpLevelData.Count;

        for (int i = 0; i < numberOfLevels; i++)
        {
            GameObject currentSlot = Instantiate(slotPrefab, scrollViewContent.transform);
            if (i > levelManagerCount)
            {
                currentSlot.GetComponent<LevelSlot>().InitializeLevelSlot(i, true);
            }
            else
            {
                currentSlot.GetComponent<LevelSlot>().InitializeLevelSlot(i);
            }
        }
    }

    public void ReturnToMainMenu()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu)
            .Unload(SceneDatabase.Scenes.GamesMenu)
            .WithOverlay()
            .Perform();
    }
}

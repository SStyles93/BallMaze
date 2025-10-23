using Newtonsoft.Json.Serialization;
using UnityEngine;

public class GamesMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject scrollViewContent;
    [SerializeField] private GameObject slotPrefab;

    [SerializeField] private int numberOfLevels = 10;

    private void Start()
    {
        InitializeSlots(numberOfLevels);
    }

    private void InitializeSlots(int numberOfLevels)
    {
        for (int i = 0; i < numberOfLevels; i++)
        {
            GameObject currentSlot = Instantiate(slotPrefab, scrollViewContent.transform);
            currentSlot.GetComponent<LevelSlot>().InitializeLevelSlot(i);
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

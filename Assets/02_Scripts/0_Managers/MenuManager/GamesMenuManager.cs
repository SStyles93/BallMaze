using UnityEngine;
using UnityEngine.UI;

public class GamesMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject scrollViewContent;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private ScrollbarData_SO scrollbarData;
    [SerializeField] private GameObject slotPrefab;

    [SerializeField] private int numberOfLevels = 10;

    #region Singleton
    public static GamesMenuManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

    }
    #endregion


    private void Start()
    {
        SavingManager.Instance.LoadSession();

        int levelManagerCount = LevelManager.Instance.KvpLevelData.Count;
        if (levelManagerCount % 10 == 0 || levelManagerCount <= 0 || levelManagerCount > numberOfLevels)
        {
            numberOfLevels = LevelManager.Instance.KvpLevelData.Count + 10;
        }
        InitializeSlots(numberOfLevels);

        scrollbar.value = scrollbarData.scrollbarValue;
        scrollbar.size = scrollbarData.scrollbarSize;
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
        SaveScrollbarValues();

        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu)
            .Unload(SceneDatabase.Scenes.GamesMenu)
            .WithOverlay()
            .Perform();
    }

    public void SaveScrollbarValues()
    {
        scrollbarData.SetScrollbarValues(scrollbar);
    }
}

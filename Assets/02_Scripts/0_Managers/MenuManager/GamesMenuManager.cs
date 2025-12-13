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
        if (SavingManager.Instance == null) 
            Debug.Log("Saving Manager does not exist");    
        SavingManager.Instance?.LoadSession();
        AudioManager.Instance?.PlayMusic();

        int levelManagerCount = LevelManager.Instance.LevelDataDictionnary.Count;
        if (levelManagerCount % 10 == 0 || levelManagerCount <= 0 || levelManagerCount > numberOfLevels)
        {
            numberOfLevels = LevelManager.Instance.LevelDataDictionnary.Count + 10;
        }
        InitializeSlots(numberOfLevels);

        scrollbar.value = scrollbarData.scrollbarValue;
        scrollbar.size = scrollbarData.scrollbarSize;
    }

    private void InitializeSlots(int numberOfLevels)
    {
        int levelManagerCount = LevelManager.Instance.LevelDataDictionnary.Count;

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

    public void OpenShopMenu()
    {
        SaveScrollbarValues();

        //SceneController.Instance
        //    .NewTransition()
        //    .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.Shop)
        //    .Unload(SceneDatabase.Scenes.GamesMenu)
        //    .WithOverlay()
        //    .Perform();
    }

    public void OpenSettingsMenu()
    {
        SaveScrollbarValues();

    }

    public void OpenCustomizationMenu()
    {
        SaveScrollbarValues();

        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.CustomizationMenu)
            .Unload(SceneDatabase.Scenes.GamesMenu)
            .WithOverlay()
            .Perform();
    }

    public void SaveScrollbarValues()
    {
        scrollbarData.SetScrollbarValues(scrollbar);
    }
}

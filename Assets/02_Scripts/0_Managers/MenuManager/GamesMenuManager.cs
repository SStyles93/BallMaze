using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GamesMenuManager : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private GameObject scrollViewContent;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private ScrollbarData_SO scrollbarData;
    [SerializeField] private GameObject slotPrefab;

    [Header("Variables")]
    [SerializeField] private int numberOfLevels = 50;

    public static GamesMenuManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

        if (AdsManager.Instance.BannerAd != null)
            AdsManager.Instance.BannerAd.DestroyAd();

        AdsManager.Instance.LoadBanner();

    }
    private void Start()
    {
        AdsManager.Instance.BannerAd.ShowAd();

        if (SavingManager.Instance == null)
            Debug.Log("Saving Manager does not exist");
        SavingManager.Instance?.LoadSession();
        AudioManager.Instance?.PlayMusic();


        int levelManagerCount = LevelManager.Instance.LevelDataDictionnary.Count;
        if (levelManagerCount % numberOfLevels == 0 || levelManagerCount <= 0 || levelManagerCount > numberOfLevels)
        {
            numberOfLevels = LevelManager.Instance.LevelDataDictionnary.Count + numberOfLevels;
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
                bool lockLevel = true;
#if UNITY_EDITOR
                if (CoreManager.Instance.unlockAllLevels) lockLevel = false;
#endif
                currentSlot.GetComponent<LevelSlot>().InitializeLevelSlot(i + 1, lockLevel);
            }
            else
            {
                currentSlot.GetComponent<LevelSlot>().InitializeLevelSlot(i + 1);
            }
        }
    }

    public void OpenShopMenu()
    {
        SaveScrollbarValues();
        SavingManager.Instance.SaveGame();

        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.ShopMenu)
            .Unload(SceneDatabase.Scenes.GamesMenu)
            .WithOverlay()
            .Perform();
    }

    public void OpenSettingsPannel()
    {
        SaveScrollbarValues();
        SavingManager.Instance.SaveGame();

        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.SettingsPannel)
            .Perform();
    }

    public void OpenCustomizationMenu()
    {
        SaveScrollbarValues();
        SavingManager.Instance.SaveGame();

        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.CustomizationMenu)
            .Unload(SceneDatabase.Scenes.GamesMenu)
            .WithOverlay()
            .Perform();
    }

    public void OpenHeartPannel()
    {
        SaveScrollbarValues();
        SavingManager.Instance.SaveGame();

        //Load Rewarded ads before the pannel appears
        AdsManager.Instance?.RewardedVideoAd.LoadAd();

        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.HeartPannel)
            .Perform();
    }

    public void SaveScrollbarValues()
    {
        scrollbarData.SetScrollbarValues(scrollbar);
    }
}

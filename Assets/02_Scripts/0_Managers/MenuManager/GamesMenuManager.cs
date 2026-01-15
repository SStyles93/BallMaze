using System;
using UnityEngine;
using UnityEngine.UI;

public class GamesMenuManager : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private GameObject scrollViewContent;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private ScrollbarData_SO scrollbarData;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private PlayButton playButton;

    public static GamesMenuManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (SavingManager.Instance == null)
            Debug.Log("Saving Manager does not exist");
        SavingManager.Instance?.LoadSession();
        AudioManager.Instance?.PlayMusic();

        InitializeSlots();

        scrollbar.value = scrollbarData.scrollbarValue;
        scrollbar.size = scrollbarData.scrollbarSize;

        playButton.InitializeLastLevelToPlay();
    }

    public void SetLevelToPlay(int index)
    {
        playButton.SetIndexOfLevelToPlay(index);
    }

    private void InitializeSlots()
    {
        int highestFinishedLevel = LevelManager.Instance.GetHighestFinishedLevelIndex();

        // Show up to next playable level (+2: 1 for the next level, 1 for teaser (locked)
        int slotsToShow = Mathf.Max(highestFinishedLevel + 40, 1);

        for (int i = 1; i <= slotsToShow; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, scrollViewContent.transform);
            bool isLocked;

            if (i == 1)
            {
                // First level is always playable
                isLocked = false;
            }
            else
            {
                // Locked if previous level was NOT finished
                isLocked = i - 1 > highestFinishedLevel;
            }

#if UNITY_EDITOR
            if (CoreManager.Instance.unlockAllLevels)
                isLocked = false;
#endif

            slotObj.GetComponent<LevelSlot>().InitializeLevelSlot(i, isLocked);
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

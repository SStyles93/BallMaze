using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GamesMenuManager : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private GameObject scrollViewContent;
    [SerializeField] private ScrollRect scrollRect;
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
        InitializeSlots();

        StartCoroutine(ScrollToCurrentLevel());

        playButton.InitializeLastLevelToPlay();
    }

    public void SetLevelToPlay(int index)
    {
        playButton.SetIndexOfLevelToPlay(index);
    }

    private IEnumerator ScrollToCurrentLevel()
    {
        // wait for layout + content size fitter
        yield return null;
        Canvas.ForceUpdateCanvases();

        int highestFinishedLevel = LevelManager.Instance.GetHighestFinishedLevelIndex();

        // Next playable level (1-based → 0-based)
        int currentLevelIndex = Mathf.Max(0, highestFinishedLevel);

        RectTransform contentRT = scrollViewContent.GetComponent<RectTransform>();
        RectTransform viewportRT = scrollRect.viewport;
        GridLayoutGroup grid = scrollViewContent.GetComponent<GridLayoutGroup>();

        int columns = grid.constraintCount;
        float rowHeight = grid.cellSize.y + grid.spacing.y;

        // Compute row index
        int levelRow = currentLevelIndex / columns;

        // One extra row above
        int targetRow = Mathf.Max(0, levelRow - 3);

        float targetY = targetRow * rowHeight;
        float maxScroll = contentRT.rect.height - viewportRT.rect.height;

        if (maxScroll <= 0f)
        {
            scrollRect.verticalNormalizedPosition = 1f;
            yield break;
        }

        float normalizedPos = 1f - Mathf.Clamp01(targetY / maxScroll);
        scrollRect.verticalNormalizedPosition = normalizedPos;
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
        SavingManager.Instance.SaveGame();

        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.SettingsPannel)
            .Perform();
    }

    public void OpenCustomizationMenu()
    {
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
        SavingManager.Instance.SaveGame();

        //Load Rewarded ads before the pannel appears
        AdsManager.Instance?.RewardedHeartsVideoAd.LoadAd();

        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.HeartPannel)
            .Perform();
    }
}

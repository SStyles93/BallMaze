using UnityEngine;
using UnityEngine.SceneManagement;

public class CoreManager : MonoBehaviour
{
    [Header("DEBUG")]
    [SerializeField] public bool isDebugPlay = false;
    [SerializeField] public bool unlockAllLevels = false;
    [SerializeField] public int numberOfLevels = 100;

    public static CoreManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (SavingManager.Instance == null)
            Debug.Log("Saving Manager does not exist");
        SavingManager.Instance?.LoadSession();

        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.StartMenu)
            .Perform();
    }
}

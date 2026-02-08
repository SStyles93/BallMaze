using UnityEngine;


public enum GameState
{
    Playing,
    WaitingForContinue,
    Paused
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    public GameState CurrentGameState => currentGameState;

    [SerializeField] private GameState currentGameState = GameState.Playing;
    
    

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        currentGameState = GameState.Playing;
    }

    public void SetState(GameState state)
    {
        currentGameState = state;
    }

    public void PauseGame() => currentGameState = GameState.Paused;
    public void ResumeGame() => currentGameState = GameState.Playing;
}

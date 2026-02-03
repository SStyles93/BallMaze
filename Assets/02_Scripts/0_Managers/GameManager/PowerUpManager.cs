using System;
using UnityEngine;
using UnityEngine.Rendering;
public enum PowerUpState
{
    Using,
    Clear
}

public class PowerUpManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private PlayerCamera playerCamera;

    [Header("Player Reference")]
    private GameObject player;

    [Header("PowerUp Prefabs")]
    [SerializeField] private GameObject rocketObject;
    [SerializeField] private double rocketTimer = 5.0;
    [Space(10)]
    [SerializeField] private GameObject ufoObject;
    [SerializeField] private double ufoTimer = 10.0;

    private PlayerState playerState;

    [Space(10)]
    [SerializeField] private double currentPowerUpTimer = 0;
    private PowerUpState powerUpState = PowerUpState.Clear;
    private CoinType currentPowerType;

    public PlayerState PlayerState => playerState;
    public PowerUpState CurrentPowerUpState => powerUpState;

    public event Action<PowerUpState> OnPowerUpStateChanged;

    public static PowerUpManager Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if(playerCamera == null)
            playerCamera = FindAnyObjectByType<PlayerCamera>();
    }

    private void Update()
    {
        if (currentPowerUpTimer > 0)
            currentPowerUpTimer -= Time.deltaTime;
        else
        {
            if (powerUpState == PowerUpState.Using)
            {
                SetPowerUpState(PowerUpState.Clear);
                //Disable PowerUps
                switch (currentPowerType)
                {
                    case CoinType.ROCKET:
                        player.transform.position = rocketObject.transform.position;
                        rocketObject.SetActive(false);
                        break;
                    case CoinType.UFO:
                        player.transform.position = ufoObject.transform.position;
                        ufoObject.SetActive(false);
                        break;
                    default:
                        break;
                }
                PlayerCamera.SetCameraFollow(player);
                player.SetActive(true);
            }
        }
    }

    public void SetPlayer(GameObject player)
    {
        this.player = player;
    }

    public void UsePowerUp(CoinType powerType)
    {
        if (powerUpState == PowerUpState.Using || playerState != PlayerState.Alive) return;

        switch (powerType)
        {
            case CoinType.ROCKET:
                currentPowerUpTimer = rocketTimer;
                player.SetActive(false);
                Vector3 rocketPosition = player.transform.position;
                rocketPosition.y = 4.7f;
                rocketObject.transform.position = rocketPosition;
                PlayerCamera.SetCameraFollow(rocketObject);
                rocketObject.SetActive(true);
                break;

            case CoinType.UFO:
                currentPowerUpTimer = ufoTimer;
                player.SetActive(false);
                Vector3 ufoPosition = player.transform.position;
                ufoPosition.y = 3.6f;
                ufoObject.transform.position = ufoPosition;
                PlayerCamera.SetCameraFollow(ufoObject);
                ufoObject.SetActive(true);
                break;

            default:
                Debug.LogWarning($"No PowerUp of type {powerType}");
                return;
        }
        currentPowerType = powerType;
        SetPowerUpState(PowerUpState.Using);
    }

    private void SetPowerUpState(PowerUpState state)
    {
        powerUpState = state;
        OnPowerUpStateChanged?.Invoke(state);
    }
}

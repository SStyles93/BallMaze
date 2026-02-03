using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum PowerUpState
{
    Using,
    Clear
}

[Serializable]
public class PowerUpData
{
    public CoinType type;
    public GameObject objectRef;
    public float duration;
    public float heightOffset;

    [HideInInspector] public Vector3 originalScale;
}

public class PowerUpManager : MonoBehaviour
{
    [Header("Player")]
    private GameObject player;
    private Vector3 playerOriginalScale;

    [Header("PowerUps")]
    [SerializeField] private PowerUpData rocket;
    [SerializeField] private PowerUpData ufo;

    private Dictionary<CoinType, PowerUpData> powerUps;

    [Header("Scale Animation")]
    [SerializeField] private float scaleDuration = 0.25f;
    [SerializeField] private Ease easeIn = Ease.InBack;
    [SerializeField] private Ease easeOut = Ease.OutBack;
    [SerializeField] private float squashStretch = 1.15f;

    private static readonly Vector3 HiddenScale = Vector3.zero;

    private float currentPowerUpTimer;
    private CoinType currentPowerType;
    private PowerUpState powerUpState = PowerUpState.Clear;
    private PlayerState playerState;

    public PowerUpState CurrentPowerUpState => powerUpState;

    public PlayerState PlayerState => playerState;

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

        powerUps = new Dictionary<CoinType, PowerUpData>
        {
            { rocket.type, rocket },
            { ufo.type, ufo }
        };

        foreach (var pu in powerUps.Values)
        {
            pu.originalScale = pu.objectRef.transform.localScale;
            pu.objectRef.SetActive(false);
        }
    }

    private void Update()
    {
        if (powerUpState != PowerUpState.Using)
            return;

        currentPowerUpTimer -= Time.deltaTime;

        if (currentPowerUpTimer <= 0f)
        {
            DeactivatePowerUp();
            SetPowerUpState(PowerUpState.Clear);
        }
    }

    public void SetPlayer(GameObject player)
    {
        this.player = player;
        playerOriginalScale = player.transform.localScale;
    }

    public void UsePowerUp(CoinType type)
    {
        if (powerUpState == PowerUpState.Using || playerState != PlayerState.Alive)
            return;

        if (!powerUps.TryGetValue(type, out var pu))
        {
            Debug.LogWarning($"No PowerUp of type {type}");
            return;
        }

        currentPowerType = type;
        currentPowerUpTimer = pu.duration;

        ActivatePowerUp(pu);
        SetPowerUpState(PowerUpState.Using);
    }

    private void ActivatePowerUp(PowerUpData pu)
    {
        player.transform.DOKill();
        pu.objectRef.transform.DOKill();
        Rigidbody puRb = pu.objectRef.GetComponent<Rigidbody>();

        // Position power-up
        Vector3 pos = player.transform.position;
        pos.y = pu.heightOffset;
        pu.objectRef.transform.position = pos;
        pu.objectRef.transform.localScale = HiddenScale;
        PlayerCamera.SetCameraFollow(pu.objectRef);

        Sequence seq = DOTween.Sequence();
        
        // Block player
        player.GetComponent<Rigidbody>().isKinematic = true;

        // Shrink player
        seq.Append(
            player.transform
            .DOScale(HiddenScale, scaleDuration)
            .SetEase(easeIn)
            .OnComplete(() => player.SetActive(false)));

        // Disable Player
        seq.AppendCallback(() =>
        {
            pu.objectRef.SetActive(true);            
        });

        // Power-up grow + squash
        seq.Append(
            pu.objectRef.transform
            .DOScale(pu.originalScale * squashStretch, scaleDuration)
            .SetEase(easeOut)
            .OnComplete(() => pu.objectRef.transform.DOScale(pu.originalScale, 0.1f)));

        // Enable Power-up
        seq.AppendCallback(() =>
        {
            puRb.isKinematic = false;
            Camera.main.transform
                .DOPunchPosition(Vector3.up * 0.3f, 0.15f);
        });
    }

    private void DeactivatePowerUp()
    {
        var pu = powerUps[currentPowerType];
        player.transform.DOKill();
        pu.objectRef.transform.DOKill();

        Rigidbody puRb = pu.objectRef.GetComponent<Rigidbody>();

        // Restore player position
        player.transform.position = pu.objectRef.transform.position;
        PlayerCamera.SetCameraFollow(player);
        player.transform.localScale = HiddenScale;

        Sequence seq = DOTween.Sequence();

        //Block pu
        puRb.isKinematic = true;

        // Shrink Power-up
        seq.Append(
            pu.objectRef.transform
                .DOScale(HiddenScale, scaleDuration)
                .SetEase(easeIn)
                .OnComplete(() => pu.objectRef.SetActive(false))
        );

        // Enable player
        seq.AppendCallback(() =>
        {
            player.SetActive(true);
        });

        // Player grows + squash
        seq.Append(
            player.transform
                .DOScale(playerOriginalScale * squashStretch, scaleDuration)
                .SetEase(easeOut)
                .OnComplete(() => player.transform.DOScale(playerOriginalScale, 0.1f)));

        seq.AppendCallback(() =>
        {
            player.GetComponent<Rigidbody>().isKinematic = false;
            Camera.main.transform
                .DOPunchPosition(Vector3.up * 0.15f, 0.12f);
        });

    }

    private void SetPowerUpState(PowerUpState state)
    {
        powerUpState = state;
        OnPowerUpStateChanged?.Invoke(state);
    }
}

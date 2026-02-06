using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum PowerUpState
{
    Using,
    Clear,
    Blocked
}

[Serializable]
public class PowerUpData
{
    public CoinType type;
    public GameObject objectRef;
    public float duration;
    public float heightOffset;

    public int buyingValue;

    [HideInInspector] public Vector3 originalScale;
}

public class PowerUpManager : MonoBehaviour
{
    [Header("Scene Refs")]
    [SerializeField] private PhysicalMazeGenerator physicalMazeGeneratorRef;

    [Header("Player")]
    private GameObject player;
    private Vector3 playerOriginalScale;

    [Header("PowerUps")]
    [SerializeField] private PowerUpData rocket;
    [SerializeField] private PowerUpData ufo;


    [Header("Scale Animation")]
    [SerializeField] private float scaleDuration = 0.25f;
    [SerializeField] private Ease easeIn = Ease.InBack;
    [SerializeField] private Ease easeOut = Ease.OutBack;
    [SerializeField] private float squashStretch = 1.15f;

    private Dictionary<CoinType, PowerUpData> powerUps;
    private static readonly Vector3 HiddenScale = Vector3.zero;
    private float currentPowerUpTimer;
    private CoinType currentPowerType;
    private PowerUpState powerUpState = PowerUpState.Clear;
    private PlayerState playerState;
    private Sequence sequence;
    private Tween playerScaleTween;
    private Tween powerUpScaleTween;

    public PowerUpState CurrentPowerUpState => powerUpState;

    public PlayerState PlayerState => playerState;

    public event Action<PowerUpState> OnPowerUpStateChanged;

    private void Awake()
    {
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

        if (physicalMazeGeneratorRef == null) physicalMazeGeneratorRef = FindAnyObjectByType<PhysicalMazeGenerator>();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        SetPlayer(player);
    }

    private void Update()
    {
        if (powerUpState == PowerUpState.Using)
        {
            currentPowerUpTimer -= Time.deltaTime;

            if (currentPowerUpTimer <= 0f)
            {
                DeactivatePowerUp();
                SetPowerUpState(PowerUpState.Clear);
            }
        }
    }

    public float GetPowerUpDuration(CoinType type)
    {
        return powerUps.TryGetValue(type, out var pu) ?
            pu.duration : 0f;
    }

    public float GetPowerUpHeightOffset(CoinType type)
    {
        return powerUps.TryGetValue(type, out var pu) ?
            pu.heightOffset : 0f;
    }

    public int GetPowerUpBuyingValue(CoinType type)
    {
        return powerUps.TryGetValue(type, out var pu) ?
            pu.buyingValue : 9999;
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
        playerScaleTween?.Kill();
        powerUpScaleTween?.Kill();
        sequence?.Kill();

        sequence = DOTween.Sequence().SetTarget(this);


        Rigidbody puRb = pu.objectRef.GetComponent<Rigidbody>();

        // Position power-up
        Vector3 pos = player.transform.position;
        pos.y = pu.heightOffset;
        pu.objectRef.transform.position = pos;
        pu.objectRef.transform.localScale = HiddenScale;
        PlayerCamera.SetCameraFollow(pu.objectRef);

        sequence?.Kill();
        sequence = DOTween.Sequence();

        // Block player
        player.GetComponent<Rigidbody>().isKinematic = true;

        // Shrink player
        playerScaleTween = player.transform
            .DOScale(HiddenScale, scaleDuration)
            .SetEase(easeIn)
            .OnComplete(() => player.SetActive(false));

        sequence.Append(playerScaleTween);

        // Disable Player
        sequence.AppendCallback(() =>
        {
            pu.objectRef.SetActive(true);
        });

        // Power-up grow + squash

        powerUpScaleTween = pu.objectRef.transform
            .DOScale(pu.originalScale * squashStretch, scaleDuration)
            .SetEase(easeOut)
            .OnComplete(() => 
                pu.objectRef.transform.DOScale(pu.originalScale, 0.1f)
            );

        sequence.Append(powerUpScaleTween);

        // Enable Power-up
        sequence.AppendCallback(() =>
        {
            puRb.isKinematic = false;
            Camera.main.transform
                .DOPunchPosition(Vector3.up * 0.3f, 0.15f);
        });
    }

    private void DeactivatePowerUp()
    {
        playerScaleTween?.Kill();
        powerUpScaleTween?.Kill();
        sequence?.Kill();
        sequence = DOTween.Sequence().SetTarget(this);

        
        var pu = powerUps[currentPowerType];

        Rigidbody puRb = pu.objectRef.GetComponent<Rigidbody>();

        // Restore player position
        player.transform.position = pu.objectRef.transform.position;
        PlayerCamera.SetCameraFollow(player);
        player.transform.localScale = HiddenScale;

        //Block pu
        puRb.isKinematic = true;

        // Shrink Power-up
        powerUpScaleTween = pu.objectRef.transform
            .DOScale(HiddenScale, scaleDuration)
            .SetEase(easeIn)
            .OnComplete(() => pu.objectRef.SetActive(false));

        sequence.Append(powerUpScaleTween);

        // Enable player
        sequence.AppendCallback(() =>
        {
            player.SetActive(true);
        });

        playerScaleTween = player.transform
            .DOScale(playerOriginalScale * squashStretch, scaleDuration)
            .SetEase(easeOut)
            .OnComplete(() =>
                    player.transform.DOScale(playerOriginalScale, 0.1f)
                );

        sequence.Append(playerScaleTween);

        sequence.AppendCallback(() =>
        {
            player.GetComponent<Rigidbody>().isKinematic = false;
            Camera.main.transform
                .DOPunchPosition(Vector3.up * 0.15f, 0.12f);
            pu.objectRef.transform.rotation = Quaternion.identity;
        });
    }

    private void SetPowerUpState(PowerUpState state)
    {
        powerUpState = state;
        OnPowerUpStateChanged?.Invoke(state);
    }

    private void SetPlayer(GameObject player)
    {
        this.player = player;
        playerOriginalScale = player.transform.localScale;
    }

    private void OnDisable()
    {
        DOTween.Kill(this); // kills only tweens targeting this manager
        playerScaleTween?.Kill();
        powerUpScaleTween?.Kill();
    }
}

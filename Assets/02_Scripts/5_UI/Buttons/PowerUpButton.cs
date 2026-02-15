using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpButton : UIButton
{
    [Header("Scene References")]
    [SerializeField] private PowerUpBuyPannel powerUpBuyPannel;
    [SerializeField] private PowerUpManager powerUpManager;

    [SerializeField] private CoinType powerType;

    [Header("Visual ELements")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private Image plusImage;

    PowerUpState m_powerUpState = PowerUpState.Clear;
    PlayerState m_playerState = PlayerState.Alive;

    bool isLockedByDistance = false;
    bool isLockedByEnd = false;
    bool isUnlocked = false;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        powerUpManager.OnPowerUpStateChanged += SetPowerUpState;

        PlayerMovement.OnPlayerStateChanged += SetPlayerState;
        PowerUpDistanceChecker.OnPowerUpBlocked += SetLockByDistance;
        EndTrigger.OnPowerUpBlocked += SetLockByEnd;


        button.onClick.AddListener(TryUsePowerUp);

        UpdatePowerUpVisuals(CoinManager.Instance.GetCoinAmount(powerType));
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        powerUpManager.OnPowerUpStateChanged -= SetPowerUpState;

        PlayerMovement.OnPlayerStateChanged -= SetPlayerState;
        PowerUpDistanceChecker.OnPowerUpBlocked -= SetLockByDistance;
        EndTrigger.OnPowerUpBlocked += SetLockByEnd;

        button.onClick.RemoveListener(TryUsePowerUp);
    }

    public void UpdatePowerUpAmout()
    {
        UpdatePowerUpVisuals(CoinManager.Instance.GetCoinAmount(powerType));
    }

    private void TryUsePowerUp()
    {
        if (powerUpManager == null || CoinManager.Instance == null) return;

        if (powerUpManager.PlayerState != PlayerState.Alive) return;

        if (!isUnlocked) return;

        if (CoinManager.Instance.CanAfford(powerType, 1))
        {
            CoinManager.Instance.ReduceCurrencyAmount(powerType, 1);
            SavingManager.Instance.SavePlayer();

            UpdatePowerUpVisuals(CoinManager.Instance.GetCoinAmount(powerType));
            powerUpManager.UsePowerUp(powerType);
        }
        else
        {
            powerUpBuyPannel.gameObject.SetActive(true);
            int powerUpValue = powerUpManager.GetPowerUpBuyingValue(powerType);
            powerUpBuyPannel.InitializePowerUpBuyPannel(this, powerType, powerUpValue);
        }
    }

    private void UpdatePowerUpVisuals(int powerUpAmount)
    {
        bool hasPowerUp = powerUpAmount > 0;
        if (hasPowerUp)
        {
            amountText.enabled = true;
            amountText.text = powerUpAmount.ToString();
            plusImage.enabled = false;
        }
        else
        {
            amountText.enabled = false;
            plusImage.enabled = true;
        }

        RefreshButtonState();
    }

    private void SetLockByDistance(bool locked)
    {
        isLockedByDistance = locked;
        RefreshButtonState();
    }
    private void SetLockByEnd()
    {
        isLockedByEnd = true;
        RefreshButtonState();
    }

    private void SetPowerUpState(PowerUpState powerUpState)
    {
        m_powerUpState = powerUpState;
        RefreshButtonState();
    }

    private void SetPlayerState(PlayerState playerState)
    {
        m_playerState = playerState;
        RefreshButtonState();
    }

    private void RefreshButtonState()
    {
        bool isPlayerAlive = m_playerState == PlayerState.Alive ? true : false;
        bool isPowerUpClear = m_powerUpState == PowerUpState.Clear ? true : false;
        isUnlocked = isPlayerAlive && isPowerUpClear && !isLockedByDistance && !isLockedByEnd;
        if (canvasGroup != null)
            canvasGroup.alpha = isUnlocked ? 1.0f : 0.1f;
    }
}

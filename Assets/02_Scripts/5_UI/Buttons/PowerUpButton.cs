using TMPro;
using UnityEngine;

public class PowerUpButton : UIButton
{
    [SerializeField] private CoinType coinType;

    [Header("Visual ELements")]
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private CanvasGroup canvasGroup;

    PowerUpState m_powerUpState = PowerUpState.Clear;
    PlayerState m_playerState = PlayerState.Alive;
    int m_powerUpAmount;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        if (PowerUpManager.Instance == null) return;
        PowerUpManager.Instance.OnPowerUpStateChanged += SetPowerUpState;
        PlayerMovement.OnPlayerStateChanged += SetPlayerState;
        
        button.onClick.AddListener(TryUsePowerUp);

        UpdatePowerUpVisuals(CoinManager.Instance.GetCoinAmount(coinType));
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (PowerUpManager.Instance == null) return;
        PowerUpManager.Instance.OnPowerUpStateChanged -= SetPowerUpState;
        PlayerMovement.OnPlayerStateChanged -= SetPlayerState;
        
        button.onClick.RemoveListener(TryUsePowerUp);
    }

    private void TryUsePowerUp()
    {
        if (PowerUpManager.Instance == null || CoinManager.Instance == null) return;

        if (PowerUpManager.Instance.PlayerState != PlayerState.Alive) return;

        if(CoinManager.Instance.CanAfford(coinType, 1))
        {
            CoinManager.Instance.ReduceCurrencyAmount(coinType, 1);
            UpdatePowerUpVisuals(CoinManager.Instance.GetCoinAmount(coinType));
            PowerUpManager.Instance.UsePowerUp(coinType);
        }
    }
    private void UpdatePowerUpVisuals(int powerUpAmount)
    {
        amountText.text = powerUpAmount > 0 ? powerUpAmount.ToString() : "0";
        m_powerUpAmount = powerUpAmount;
        RefreshButtonVisibility();
    }

    private void SetPowerUpState(PowerUpState powerUpState)
    {
        m_powerUpState = powerUpState;
        RefreshButtonVisibility();
    }

    private void SetPlayerState(PlayerState playerState)
    {
        m_playerState = playerState;
        RefreshButtonVisibility();
    }

    private void RefreshButtonVisibility()
    {
        bool isPlayerAlive = m_playerState == PlayerState.Alive ? true : false;
        bool isPowerUpClear = m_powerUpState == PowerUpState.Clear ? true : false;
        bool hasPowerUp = m_powerUpAmount > 0 ? true : false;
        canvasGroup.alpha = isPlayerAlive && isPowerUpClear && hasPowerUp ? 1.0f : 0.1f;
    }
}

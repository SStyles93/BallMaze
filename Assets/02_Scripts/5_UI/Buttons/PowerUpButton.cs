using TMPro;
using UnityEngine;

public class PowerUpButton : UIButton
{
    [SerializeField] private CoinType coinType;

    [Header("Visual ELements")]
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private CanvasGroup canvasGroup;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        if (PowerUpManager.Instance == null) return;
        PowerUpManager.Instance.OnPowerUpStateChanged += SetButtonState;
        
        button.onClick.AddListener(TryUsePowerUp);

        UpdatePowerUpVisuals(CoinManager.Instance.GetCoinAmount(coinType));
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (PowerUpManager.Instance == null) return;
        PowerUpManager.Instance.OnPowerUpStateChanged -= SetButtonState;
        
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
        canvasGroup.alpha = powerUpAmount > 0 ? 1.0f : 0.1f;
    }

    private void SetButtonState(PowerUpState state)
    {
        canvasGroup.alpha = state == PowerUpState.Clear ? 1.0f : 0.1f;
    }
}

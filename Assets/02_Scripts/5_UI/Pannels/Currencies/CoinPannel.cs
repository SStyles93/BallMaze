using TMPro;
using UnityEngine;

public class CoinPannel : MonoBehaviour
{
    [SerializeField] private TMP_Text currencyText;
    private void OnEnable()
    {
        CoinManager.Instance.OnCoinSet += SetCurrencyValue;
        CoinManager.Instance.OnCoinChanged += UpdateCurrencyValue;
    }

    private void OnDisable()
    {
        CoinManager.Instance.OnCoinSet -= SetCurrencyValue;
        CoinManager.Instance.OnCoinChanged -= UpdateCurrencyValue;
    }

    private void Start()
    {
        CoinManager coinManager = CoinManager.Instance;
        if(coinManager != null && coinManager.PreviousCoinAmount != CoinManager.Instance.CoinAmount)
        {
            UpdateCurrencyValue(CoinType.COIN, coinManager.CoinAmount, coinManager.PreviousCoinAmount);
            coinManager.LevelPreviousCoinAmount(CoinType.COIN);
        }
    }

    private void SetCurrencyValue(CoinType type, int value)
    {
        if (type == CoinType.COIN)
            currencyText.text = $"{value.ToString()}";
    }

    private void UpdateCurrencyValue(CoinType type, int value, int previousValue)
    {
        if (type == CoinType.COIN)
            currencyText.AnimateCurrency(previousValue, value, 1.0f);
    }
}

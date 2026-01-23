using UnityEngine;
using TMPro;

public class CurrencyPannel : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    protected CoinType m_coinType = CoinType.COIN;

    protected CoinManager coinManagerRef;

    protected virtual void OnEnable()
    {
        coinManagerRef = CoinManager.Instance;
        coinManagerRef.OnCoinSet += SetCurrencyValue;
        coinManagerRef.OnCoinChanged += UpdateCurrencyValue;
    }

    protected virtual void OnDisable()
    {
        coinManagerRef.OnCoinSet -= SetCurrencyValue;
        coinManagerRef.OnCoinChanged -= UpdateCurrencyValue;
    }

    protected virtual void Start()
    {
        if (coinManagerRef != null && coinManagerRef.PreviousCoinAmount != coinManagerRef.CoinAmount)
        {
            UpdateCurrencyValue(m_coinType, coinManagerRef.CoinAmount, coinManagerRef.PreviousCoinAmount);
            coinManagerRef.LevelPreviousCoinAmount(m_coinType);
        }
    }

    protected virtual void SetCurrencyValue(CoinType type, int value)
    {
        if (type != m_coinType) return;
        text.text = $"{value.ToString()}";
    }

    protected virtual void UpdateCurrencyValue(CoinType type, int value, int previousValue)
    {
        if (type != m_coinType) return;
        text.AnimateCurrency(previousValue, value, 1.0f);
    }
}

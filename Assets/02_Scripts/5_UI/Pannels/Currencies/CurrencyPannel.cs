using TMPro;
using UnityEngine;

public class CurrencyPannel : MonoBehaviour
{
    [SerializeField] private TMP_Text currencyText;

    private void OnEnable()
    {
        CoinManager.Instance.OnCoinChanged += UpdateCurrencyValue;
    }

    private void OnDisable()
    {
        CoinManager.Instance.OnCoinChanged -= UpdateCurrencyValue;
    }

    private void Start()
    {
        UpdateCurrencyValue(CoinType.COIN, CoinManager.Instance.CoinAmount);
    }

    private void UpdateCurrencyValue(CoinType type, int value)
    {
        if(type == CoinType.COIN)
        currencyText.text = $"{value.ToString()}";
    }
}

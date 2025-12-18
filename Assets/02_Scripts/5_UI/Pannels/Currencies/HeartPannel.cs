using TMPro;
using UnityEngine;

public class HeartPannel : MonoBehaviour
{
    [SerializeField] private TMP_Text currencyText;

    private void OnEnable()
    {
        CoinManager.Instance.OnCurrencyChanged += UpdateCurrencyValue;
    }

    private void OnDisable()
    {
        CoinManager.Instance.OnCurrencyChanged -= UpdateCurrencyValue;
    }

    private void UpdateCurrencyValue(CoinType type, int value)
    {
        if(type == CoinType.HEART)
        currencyText.text = $"{value.ToString()}";
    }
}

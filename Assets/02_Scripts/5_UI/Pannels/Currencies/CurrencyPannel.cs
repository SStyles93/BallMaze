using TMPro;
using UnityEngine;

public class CurrencyPannel : MonoBehaviour
{
    [SerializeField] private TMP_Text currencyText;

    private void OnEnable()
    {
        CurrencyManager.Instance.OnCoinAmountChanged += UpdateCurrencyValue;
    }

    private void OnDisable()
    {
        CurrencyManager.Instance.OnCoinAmountChanged -= UpdateCurrencyValue;
    }

    private void UpdateCurrencyValue(int value)
    {
        currencyText.text = $"{value.ToString()}";
    }
}

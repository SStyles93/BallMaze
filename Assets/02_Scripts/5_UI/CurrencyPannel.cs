using TMPro;
using UnityEngine;

public class CurrencyPannel : MonoBehaviour
{
    [SerializeField] private TMP_Text currencyText;

    private void OnEnable()
    {
        CurrencyManager.Instance.OnCurrencyChanged += UpdateCurrencyValue;
    }

    private void OnDisable()
    {
        CurrencyManager.Instance.OnCurrencyChanged -= UpdateCurrencyValue;
    }

    private void UpdateCurrencyValue(int value)
    {
        currencyText.text = $"{value.ToString()} $";
    }
}

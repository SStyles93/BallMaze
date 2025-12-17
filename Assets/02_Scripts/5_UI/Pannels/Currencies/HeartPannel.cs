using TMPro;
using UnityEngine;

public class HeartPannel : MonoBehaviour
{
    [SerializeField] private TMP_Text currencyText;

    private void OnEnable()
    {
        CurrencyManager.Instance.OnHeartAmountChanged += UpdateCurrencyValue;
    }

    private void OnDisable()
    {
        CurrencyManager.Instance.OnHeartAmountChanged -= UpdateCurrencyValue;
    }

    private void UpdateCurrencyValue(int value)
    {
        currencyText.text = $"{value.ToString()}";
    }
}

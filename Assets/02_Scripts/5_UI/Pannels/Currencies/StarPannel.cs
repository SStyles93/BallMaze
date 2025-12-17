using TMPro;
using UnityEngine;

public class StarPannel : MonoBehaviour
{
    [SerializeField] private TMP_Text currencyText;

    private void OnEnable()
    {
        CurrencyManager.Instance.OnStarAmountChanged += UpdateCurrencyValue;
    }

    private void OnDisable()
    {
        CurrencyManager.Instance.OnStarAmountChanged -= UpdateCurrencyValue;
    }

    private void UpdateCurrencyValue(int value)
    {
        currencyText.text = $"{value.ToString()}";
    }
}

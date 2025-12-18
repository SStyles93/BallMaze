using TMPro;
using UnityEngine;

public class StarPannel : MonoBehaviour
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
        if(type == CoinType.STAR)
        currencyText.text = $"{value.ToString()}";
    }
}

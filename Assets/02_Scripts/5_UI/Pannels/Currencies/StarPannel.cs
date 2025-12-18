using TMPro;
using UnityEngine;

public class StarPannel : MonoBehaviour
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

    private void UpdateCurrencyValue(CoinType type, int value)
    {
        if(type == CoinType.STAR)
        currencyText.text = $"{value.ToString()}";
    }
}

using TMPro;
using UnityEngine;

public class HeartPannel : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private GameObject timerPannel;
    [SerializeField] private TMP_Text timerText;

    [Header("Heart")]
    [SerializeField] private TMP_Text heartAmountText;
    [SerializeField] private GameObject shopButton;


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
        if(type == CoinType.HEART)
        heartAmountText.text = $"{value.ToString()}";
    }
}

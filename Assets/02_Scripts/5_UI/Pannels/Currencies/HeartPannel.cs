using System;
using TMPro;
using Unity.VisualScripting;
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
        CoinManager.Instance.OnCoinSet += SetCurrencyValue;
        CoinManager.Instance.OnCoinChanged += UpdateCurrencyValue;
        CoinManager.Instance.OnHeartTimerTick += UpdateTimerText;
    }

    private void OnDisable()
    {
        CoinManager.Instance.OnCoinSet -= SetCurrencyValue;
        CoinManager.Instance.OnCoinChanged -= UpdateCurrencyValue;
        CoinManager.Instance.OnHeartTimerTick -= UpdateTimerText;
    }

    private void Start()
    {
        CoinManager coinManager = CoinManager.Instance;
        if (coinManager!=null && coinManager.PreviousCoinAmount != CoinManager.Instance.CoinAmount)
        {
            UpdateCurrencyValue(CoinType.HEART, coinManager.HeartAmount, coinManager.PreviousHeartAmount);
            coinManager.LevelPreviousCoinAmount(CoinType.HEART);
        }
        UpdateTimerText(coinManager.TimeUntilNextHeart());
    }

    private void SetCurrencyValue(CoinType type, int value)
    {
        if (type == CoinType.HEART)
            heartAmountText.text = $"{value.ToString()}";
    }

    private void UpdateCurrencyValue(CoinType type, int value, int previousValue)
    {
        if(type == CoinType.HEART)
        heartAmountText.AnimateCurrency(previousValue, value, 1.0f);
    }

    void UpdateTimerText(TimeSpan remaining)
    {
        if (remaining <= TimeSpan.Zero)
        {
            timerText.text = "FULL";
            timerPannel.SetActive(false);
            shopButton.SetActive(false);
        }
        else
        {
            timerPannel.SetActive(true);
            shopButton.SetActive(true);
            timerText.text = $"{remaining.Minutes:00}:{remaining.Seconds:00}";
        }
    }
}

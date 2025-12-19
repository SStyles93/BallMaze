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
        CoinManager.Instance.OnCoinChanged += UpdateCurrencyValue;
        CoinManager.Instance.OnHeartTimerTick += UpdateTimerText;
    }

    private void OnDisable()
    {
        CoinManager.Instance.OnCoinChanged -= UpdateCurrencyValue;
        CoinManager.Instance.OnHeartTimerTick -= UpdateTimerText;
    }

    private void Start()
    {
        CoinManager coinManager = CoinManager.Instance;
        UpdateCurrencyValue(CoinType.HEART, coinManager.HeartAmount);
        UpdateTimerText(coinManager.TimeUntilNextHeart());
    }

    private void UpdateCurrencyValue(CoinType type, int value)
    {
        if(type == CoinType.HEART)
        heartAmountText.text = $"{value.ToString()}";
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

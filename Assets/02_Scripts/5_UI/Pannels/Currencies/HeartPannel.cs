using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class HeartPannel : CurrencyPannel
{
    [Header("Timer")]
    [SerializeField] private GameObject timerPannel;
    [SerializeField] private TMP_Text timerText;

    [Header("Buttons")]
    [SerializeField] private GameObject shopButton;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        coinManagerRef.OnHeartTimerTick += UpdateTimerText;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        coinManagerRef.OnHeartTimerTick -= UpdateTimerText;
    }

    protected override void Start()
    {
        m_coinType = CoinType.HEART;
        if (coinManagerRef == null) return;

        if (coinManagerRef.PreviousHeartAmount != coinManagerRef.HeartAmount)
        {
            UpdateCurrencyValue(m_coinType, coinManagerRef.HeartAmount, coinManagerRef.PreviousHeartAmount);
            coinManagerRef.LevelPreviousCoinAmount(m_coinType);
        }
        else
        {
            SetCurrencyValue(m_coinType, coinManagerRef.HeartAmount);
        }

        UpdateTimerText(coinManagerRef.TimeUntilNextHeart());
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

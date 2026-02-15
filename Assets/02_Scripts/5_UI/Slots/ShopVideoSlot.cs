using System;
using TMPro;
using UnityEngine;
using Unity.Services.LevelPlay;
using UnityEngine.UI;

public class ShopVideoSlot : MonoBehaviour
{
    [Header("ShopSlot Parameters")]
    [SerializeField] private TMP_Text videoTimerText;
    [SerializeField] private Button buyButton;

    private void OnEnable()
    {
        CoinManager.Instance.OnCoinTimerTick += UpdateTimerText;
    }

    private void OnDisable()
    {
        CoinManager.Instance.OnCoinTimerTick -= UpdateTimerText;
    }


    private void Start()
    {
        CoinManager coinManager = CoinManager.Instance;
        if (coinManager != null)
        {
            UpdateTimerText(coinManager.TimeUntilNextCoinVideo());
        }
    }

    void UpdateTimerText(TimeSpan remaining)
    {
        if (remaining <= TimeSpan.Zero)
        {
            buyButton.gameObject.SetActive(true);
            videoTimerText.gameObject.SetActive(false);
            videoTimerText.text = "";
        }
        else
        {
            buyButton.gameObject.SetActive(false);
            videoTimerText.gameObject.SetActive(true);
            videoTimerText.text = $"{remaining.Hours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
        }
    }

    // --- Ads ---

    public void LaunchRewardedAd()
    {
        AdsManager manager = AdsManager.Instance;
        if (manager == null) return;

        manager.ShowRewarded("rewarded_coins");
    }
}

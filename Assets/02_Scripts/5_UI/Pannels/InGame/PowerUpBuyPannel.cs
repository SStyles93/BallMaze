using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpBuyPannel : MonoBehaviour
{
    [SerializeField] private Image powerUpImage;
    [SerializeField] private TMP_Text powerUpValueText;

    [SerializeField] private Sprite[] powerUpSprites = new Sprite[2];

    PowerUpButton currentPowerUpButton;
    CoinType currentpowerType;
    int currentPowerUpValue;

    public void InitializePowerUpBuyPannel(PowerUpButton pUpButton, CoinType powerType, int value)
    {
        currentPowerUpButton = pUpButton;
        currentPowerUpValue = value;
        currentpowerType = powerType;

        int imageIndex = powerType == CoinType.ROCKET ? 0 : 1;
        powerUpImage.sprite = powerUpSprites[imageIndex];
        powerUpValueText.text = $"<sprite index=0> {value}";
    }

    // Called by the Buy button
    public void TryBuyPowerUp()
    {
        if(CoinManager.Instance.CanAfford(CoinType.COIN, currentPowerUpValue))
        {
            CoinManager.Instance.IncreaseCurrencyAmount(currentpowerType, 3);
            CoinManager.Instance.ReduceCurrencyAmount(CoinType.COIN, currentPowerUpValue);
            SavingManager.Instance.SavePlayer();
            
            currentPowerUpButton.UpdatePowerUpAmout();
            gameObject.SetActive(false);
        }
    }
}

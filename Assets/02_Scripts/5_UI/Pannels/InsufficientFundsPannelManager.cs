using System;
using TMPro;
using UnityEngine;

public class InsufficientFundsPannelManager : MonoBehaviour
{
    [SerializeField] private GameObject shopButton;
    [SerializeField] private GameObject homeButton;

    [SerializeField] private TMP_Text pannelText;

    public void InitializePannel(CoinType coinType)
    {
        switch (coinType)
        {
            case CoinType.COIN:
                shopButton.SetActive(true);
                homeButton.SetActive(false);
                pannelText.text = $"Insufficient <sprite index={(int)coinType}> go to shop ?";
                break;
            case CoinType.STAR:
                homeButton.SetActive(true);
                shopButton.SetActive(false);
                pannelText.text = $"Insufficient <sprite index={(int)coinType}> go to levels ?";
                break;
        }
    }
}

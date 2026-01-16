using TMPro;
using UnityEngine;

public class StarPannel : CurrencyPannel
{
    protected override void Start()
    {
        m_coinType = CoinType.STAR;
        if (coinManagerRef == null) return;

        if (coinManagerRef.PreviousStarAmount != coinManagerRef.StarAmount)
        {
            UpdateCurrencyValue(m_coinType, coinManagerRef.StarAmount, coinManagerRef.PreviousStarAmount);
            coinManagerRef.LevelPreviousCoinAmount(m_coinType);
        }
        else
        {
            SetCurrencyValue(m_coinType, coinManagerRef.StarAmount);
        }
    }
}

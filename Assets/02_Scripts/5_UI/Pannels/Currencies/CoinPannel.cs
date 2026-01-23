public class CoinPannel : CurrencyPannel
{
    protected override void Start()
    {
        m_coinType = CoinType.COIN;
        if (coinManagerRef == null) return;

        if (coinManagerRef.PreviousCoinAmount != coinManagerRef.CoinAmount)
        {
            UpdateCurrencyValue(m_coinType, coinManagerRef.CoinAmount, coinManagerRef.PreviousCoinAmount);
            coinManagerRef.LevelPreviousCoinAmount(m_coinType);
        }
        else
        {
            SetCurrencyValue(m_coinType, coinManagerRef.CoinAmount);
        }
    }
}


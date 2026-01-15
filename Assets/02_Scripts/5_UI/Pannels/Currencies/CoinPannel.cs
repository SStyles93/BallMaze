public class CoinPannel : CurrencyPannel
{
    protected override void Start()
    {
        m_coinType = CoinType.COIN;

        if (coinManagerRef != null && coinManagerRef.PreviousCoinAmount != coinManagerRef.CoinAmount)
        {
            UpdateCurrencyValue(m_coinType, coinManagerRef.CoinAmount, coinManagerRef.PreviousCoinAmount);
            coinManagerRef.LevelPreviousCoinAmount(m_coinType);
        }
    }
}


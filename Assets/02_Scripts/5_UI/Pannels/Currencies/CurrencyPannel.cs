using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyPannel : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] protected CoinType m_coinType = CoinType.COIN;

    [SerializeField] private Texture[] powerUpSprites = new Texture[5];

    protected CoinManager coinManagerRef;

    protected virtual void OnEnable()
    {
        coinManagerRef = CoinManager.Instance;
        coinManagerRef.OnCoinSet += SetCurrencyValue;
        coinManagerRef.OnCoinChanged += UpdateCurrencyValue;
    }

    protected virtual void OnDisable()
    {
        coinManagerRef.OnCoinSet -= SetCurrencyValue;
        coinManagerRef.OnCoinChanged -= UpdateCurrencyValue;
    }

    protected virtual void Start()
    {
        // --- TEXTURE SETTING
        GetComponentInChildren<RawImage>().texture = powerUpSprites[(int)m_coinType];

        // --- COIN MANAGER ---
        if (coinManagerRef != null)
        {
            int currentAmount = coinManagerRef.GetCoinAmount(m_coinType);
            int previousAmount = coinManagerRef.GetPreviousAmount(m_coinType);

            if (previousAmount != currentAmount)
            {
                UpdateCurrencyValue(m_coinType, currentAmount, previousAmount);
                coinManagerRef.LevelPreviousCoinAmount(m_coinType);
            }
            else
            {
                SetCurrencyValue(m_coinType, currentAmount);
            }
        }
    }

    protected virtual void SetCurrencyValue(CoinType type, int value)
    {
        if (type != m_coinType) return;
        text.text = $"{value.ToString()}";
    }

    protected virtual void UpdateCurrencyValue(CoinType type, int value, int previousValue)
    {
        if (type != m_coinType) return;
        text.AnimateCurrency(previousValue, value, 1.0f);
    }
}

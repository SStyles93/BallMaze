using UnityEngine;
public enum CoinType { COIN, STAR, HEART }

[System.Serializable]
public class CoinQuantityPair
{
    public CoinType CoinType;
    public int Amount;
}

[System.Serializable]
public class CurrencyValuePair
{
    public enum CurrencyType { USD, CAD, CHF, EUR }
    public CurrencyType currencyType;
    public float value;
}
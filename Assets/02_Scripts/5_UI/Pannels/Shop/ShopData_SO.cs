using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopData", menuName = "Shop/ShopData")]
public class ShopData_SO : ScriptableObject
{
    public ShopOption[] shopOptions;
}

[System.Serializable]
public class ShopOption
{
    public CurrencyValuePair price;
    public CoinQuantityPair[] currencyAmountPairs;

    public float Price => price.value;

    public bool Buy()
    {
        //Try buying
        return false;
    }
}


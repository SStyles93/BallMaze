using UnityEngine;
public enum CoinType 
{ 
    COIN, 
    STAR, 
    HEART, 
    ROCKET, 
    UFO 
}

[System.Serializable]
public class CoinQuantityPair
{
    public CoinType CoinType;
    public int Amount;
}
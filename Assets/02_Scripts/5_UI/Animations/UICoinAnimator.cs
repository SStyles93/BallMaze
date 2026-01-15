using DG.Tweening;
using TMPro;
using UnityEngine;

public class UICoinAnimator : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private float animationDuration = 1.0f;
    private void Start()
    {
        scoreText.AnimateCoin(
            0, LevelManager.Instance.CurrencyEarnedThisLevel,
            animationDuration, CoinType.COIN);
    }
}

using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class UICoinAnimator : MonoBehaviour
{
    [SerializeField] private UIStarAnimator starAnimator;

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private float animationDuration = 1.0f;

    public event Action<bool> OnCoinAnimationEnabled;

    private void Awake()
    {
        if (starAnimator == null) starAnimator = GetComponent<UIStarAnimator>();
        scoreText.enabled = false;
    }


    private void OnEnable()
    {
        starAnimator.OnPopFinished += PlayCoinAnimation;
    }

    private void OnDisable()
    {
        starAnimator.OnPopFinished -= PlayCoinAnimation;
    }


    private void PlayCoinAnimation() 
    {
        scoreText.enabled = true;

        if (LevelManager.Instance.CurrencyEarnedThisLevel == 0) return;

        scoreText.AnimateCoin(
            0, LevelManager.Instance.CurrencyEarnedThisLevel,
            animationDuration, CoinType.COIN,
            OnCoinAnimationEnabled);
    }
}

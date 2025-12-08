using DG.Tweening;
using TMPro;
using UnityEngine;

public class UICurrencyAnimator : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    private void Start()
    {
        int targetValue = CurrencyManager.Instance.CurrencyValue;
        AnimateCurrency(0, targetValue, 0.8f);
    }

    private void AnimateCurrency(int start, int end, float duration)
    {
        int currentValue = start;

        DOTween.To(() => currentValue, x =>
        {
            currentValue = x;
            scoreText.text = $"Gold\n{currentValue}";
        },
        end,
        duration)
        .SetEase(Ease.OutCubic);
    }
}

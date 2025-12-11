using DG.Tweening;
using TMPro;
using UnityEngine;

public class UICurrencyAnimator : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private float animationDuration = 1.0f;

    private void Start()
    {
     
        AnimateCurrency(
            CurrencyManager.Instance.PreviousCurrencyValue,
            CurrencyManager.Instance.CurrencyValue,
            animationDuration);
    }

    /// <summary>
    /// Animates the text to visually see the increase in currency value
    /// </summary>
    /// <param name="start">starting value of the annimation</param>
    /// <param name="end">ending value of the annimation</param>
    /// <param name="duration">duration of the annimation from start to end</param>
    private void AnimateCurrency(int start, int end, float duration)
    {
        int currentValue = start;

        DOTween.To(() => currentValue, x =>
        {
            currentValue = x;
            scoreText.text = currentValue.ToString();
        },
        end,
        duration)
        .SetEase(Ease.OutCubic);
    }
}

using DG.Tweening;
using System;
using TMPro;
public static class TMPTextExtensions
{
    /// <summary>
    /// Animates the text to visually see the increase in currency value
    /// </summary>
    public static Tweener AnimateCurrency(
        this TMP_Text text,
        int start,int end, 
        float duration)
    {
        text.DOKill();

        int currentValue = start;

        return DOTween.To(() => currentValue, x =>
        {
            currentValue = x;
            text.text = currentValue.ToString();
        },
        end,
        duration)
        .SetEase(Ease.OutCubic);
    }

    /// <summary>
    /// Animates the text to visually see the increase in currency value
    /// </summary>
    public static Tweener AnimateCoin(
        this TMP_Text text,
        int start, int end,
        float duration, CoinType coinType,
        Action<bool> onAnimationEnabled = null)
    {
        text.DOKill();

        onAnimationEnabled?.Invoke(true);

        int currentValue = start;

        return DOTween.To(() => currentValue, x =>
        {
            currentValue = x;
            text.text = $"<sprite index={(int)coinType}> {currentValue}";
        },
        end,
        duration)
        .SetEase(Ease.OutCubic)
        .OnComplete(() =>
        {
            onAnimationEnabled?.Invoke(false);
        });
    }
}
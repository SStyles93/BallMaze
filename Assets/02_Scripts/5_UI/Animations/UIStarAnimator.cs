using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIStarAnimator : MonoBehaviour
{
    [SerializeField] private List<Image> starImages = new List<Image>();
    private int numberOfStars;

    private void Start()
    {
        numberOfStars = LevelManager.Instance.CurrentLevelData.numberOfStars;

        // Disable visuals & set scale to 0
        foreach (var star in starImages)
        {
            star.enabled = false;
            star.transform.localScale = Vector3.zero;
            var color = star.color;
            color.a = 0f;
            star.color = color;
        }

        StartCoroutine(AnimateStars());
    }

    private IEnumerator AnimateStars()
    {
        for (int i = 0; i < numberOfStars; i++)
        {
            Image star = starImages[i];
            star.enabled = true;

            // DOTween pop + fade
            star.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
            star.DOFade(1f, 0.3f);

            yield return new WaitForSeconds(0.2f);
        }
    }
}

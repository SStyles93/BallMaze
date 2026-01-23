using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIStarAnimator : MonoBehaviour
{
    [SerializeField] private List<Image> starImages = new List<Image>();
    [SerializeField] private ParticleSystem[] starParticles = new ParticleSystem[2];

    private int numberOfStars;

    private void OnEnable()
    {
        numberOfStars = LevelManager.Instance.CurrentStarCount;

        StopAllCoroutines();
        ResetStars();
        StartCoroutine(AnimateStars());
    }

    private void ResetStars()
    {
        // Disable visuals & set scale to 0
        for (int i = 0; i < starImages.Count; i++)
        {
            if (i % 3 == 2)
            {
                starImages[i].enabled = false;
                starImages[i].transform.localScale = Vector3.zero;
                var color = starImages[i].color;
                color.a = 0f;
                starImages[i].color = color;
            }
        }
    }

    private IEnumerator AnimateStars()
    {
        for (int i = 0; i < numberOfStars * 3; i += 3)
        {
            // DOTween pop + fade

            Image starGlow1 = starImages[i];
            starGlow1.enabled = true;

            Image starGlow2 = starImages[i + 1];
            starGlow2.enabled = true;


            Image star = starImages[i + 2];
            star.enabled = true;
            star.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
            star.DOFade(1f, 0.3f);

            yield return new WaitForSeconds(0.2f);
        }

        if (numberOfStars >= 3 && starParticles.Length > 0)
            foreach (var particle in starParticles)
            {
                particle.Play();
            }
    }
}

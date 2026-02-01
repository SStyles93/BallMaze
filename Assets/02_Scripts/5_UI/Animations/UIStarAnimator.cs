using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIStarAnimator : MonoBehaviour
{
    [SerializeField] private List<Image> starImages = new List<Image>();
    [SerializeField] private ParticleSystem[] starParticles = new ParticleSystem[2];

    [SerializeField] private float delayBetweenStarPop = 0.5f;

    private int numberOfStars;

    public event Action OnStarPop;
    public event Action OnPopFinished;

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
            if (i % 3 == 0)
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

            Image star = starImages[i];
            star.enabled = true;
            star.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
            star.DOFade(1f, 0.3f);

            OnStarPop?.Invoke();
            yield return new WaitForSeconds(delayBetweenStarPop);

            Image starGlow1 = starImages[i + 1];
            starGlow1.enabled = true;

            Image starGlow2 = starImages[i + 2];
            starGlow2.enabled = true;
        }

        OnPopFinished?.Invoke();

        if (numberOfStars >= 3 && starParticles.Length > 0)
            foreach (var particle in starParticles)
            {
                particle.Play();
            }
    }
}

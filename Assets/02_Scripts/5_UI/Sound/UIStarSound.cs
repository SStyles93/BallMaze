using UnityEngine;

[RequireComponent(typeof(UIStarAnimator))]
public class UIStarSound : MonoBehaviour
{

    [SerializeField] private AudioSource starAudioSource;
    [SerializeField] private UIStarAnimator starAnimator;

    [SerializeField] private AudioClip starPopClip;
    [SerializeField] private AudioClip starGlitterClip;

    private void Awake()
    {
        if(starAnimator == null) starAnimator = GetComponent<UIStarAnimator>();
    }


    private void OnEnable()
    {
        starAnimator.OnStarPop += PlayStarPopSound;
        starAnimator.OnPopFinished += PlayStarSound;
    }

    private void OnDisable()
    {
        starAnimator.OnStarPop -= PlayStarPopSound;
        starAnimator.OnPopFinished -= PlayStarSound;
    }

    private void PlayStarPopSound()
    {
        if (starAudioSource.isPlaying)
        {
            starAudioSource.Stop();
        }
        starAudioSource.volume = 0.4f;
        starAudioSource.PlayOneShot(starPopClip);
    }

    private void PlayStarSound()
    {
        if (starAudioSource.isPlaying)
        {
            starAudioSource.Stop();
        }
        starAudioSource.clip = starGlitterClip;
        starAudioSource.volume = 0.3f;
        starAudioSource.loop = true;
        starAudioSource.Play();
    }
}

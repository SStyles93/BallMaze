using UnityEngine;

[RequireComponent(typeof(UICoinAnimator))]
public class UICoinSound : MonoBehaviour
{
    [SerializeField] private AudioSource coinAudioSource;
    [SerializeField] private UICoinAnimator coinAnimator;

    [SerializeField] private AudioClip coinPopClip;

    private void Awake()
    {
        if (coinAnimator == null) coinAnimator = GetComponent<UICoinAnimator>();
    }


    private void OnEnable()
    {
        coinAnimator.OnCoinAnimationEnabled += PlayCoinSound;
    }

    private void OnDisable()
    {
        coinAnimator.OnCoinAnimationEnabled -= PlayCoinSound;
    }

    private void PlayCoinSound(bool enabled)
    {
        if (!this || !coinAudioSource) return;

        if (enabled)
        {
            if (coinAudioSource.isPlaying)
            {
                coinAudioSource.Stop();
            }
            coinAudioSource.clip = coinPopClip;
            coinAudioSource.volume = 0.3f;
            coinAudioSource.loop = true;
            coinAudioSource.Play();
        }
        else
        {
            coinAudioSource.Stop();
            return;
        }
    }
}

using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(PiquesTileAnimation))]
public class PiquesTileAudio : MonoBehaviour
{
    [SerializeField] private PiquesTileAnimation piquesTileAnimation;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip piquesClip;
    private void Awake()
    {
        piquesTileAnimation ??= GetComponent<PiquesTileAnimation>();
        audioSource ??= GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        piquesTileAnimation.OnPiquesRising += PlayRisingPiquesSound;
    }

    private void OnDisable()
    {
        piquesTileAnimation.OnPiquesRising -= PlayRisingPiquesSound;
    }

    private void PlayRisingPiquesSound()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();

        if (piquesClip == null)
            Debug.LogWarning($"Clip {piquesClip} is null");

        audioSource.PlayOneShot(piquesClip);
    }
}

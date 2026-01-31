using UnityEngine;

[RequireComponent(typeof(FlapingDoorsAnimation))]
public class DoorTileAudio : MonoBehaviour
{
    [SerializeField] private FlapingDoorsAnimation flapingDoorsAnimation;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip doorClip;
    private void Awake()
    {
        flapingDoorsAnimation ??= GetComponent<FlapingDoorsAnimation>();
        audioSource ??= GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        flapingDoorsAnimation.OnDoorOpening += PlayOpeningDoorsSound;
    }

    private void OnDisable()
    {
        flapingDoorsAnimation.OnDoorOpening -= PlayOpeningDoorsSound;
    }

    private void PlayOpeningDoorsSound()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();

        if (doorClip == null)
            Debug.LogWarning($"Clip {doorClip} is null");

        audioSource.PlayOneShot(doorClip);
    }
}

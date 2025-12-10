using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource playerSfxAudioSource;
    [SerializeField] private AudioSource environmentSfxAudioSource;

    [Header("Music")]
    [SerializeField] private AudioClip musicClip;

    [Header("Player SFX")]
    [SerializeField] private AudioClip jumpClip;

    [Header("Environment SFX")]
    [SerializeField] private AudioClip starClip;
    [SerializeField] private AudioClip winClip;
    private int starCount = 0;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Really not optimal and prone to error but sources should always be defined statically ([SerializeField])
        if (musicAudioSource == null || playerSfxAudioSource == null)
        {
            AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            musicAudioSource = sources[0];
            playerSfxAudioSource = sources[1];
        }
    }

    public void PlayMusic()
    {
        if (musicAudioSource == null)
        {
            Debug.LogWarning("musicAudioSource is null");
            return;
        }

        if (musicClip != null)
            musicAudioSource.clip = musicClip;

        musicAudioSource.Play();
    }

    public void PlayJumpSound()
    {
        if (playerSfxAudioSource == null)
        {
            Debug.LogWarning("playerSfxAudioSource is null");
            return;
        }

        if (playerSfxAudioSource.isPlaying)
            playerSfxAudioSource.Stop();
        if (jumpClip != null)
        {
            playerSfxAudioSource.pitch = 2.0f;
            playerSfxAudioSource.PlayOneShot(jumpClip);
        }
    }

    public void PlayStarSound()
    {
        if (environmentSfxAudioSource == null)
        {
            Debug.LogWarning("environmentSfxAudioSource is null");
            return;
        }

        if (environmentSfxAudioSource.isPlaying)
            environmentSfxAudioSource.Stop();

        //Pitch 0.8 -> 0.9 -> 1.0
        environmentSfxAudioSource.pitch = 0.8f + (starCount * 0.1f);
        starCount++;
        if (starCount > 2) starCount = 0;

        if (starClip != null)
            environmentSfxAudioSource.PlayOneShot(starClip);
    }

    public void PlayWinSound() 
    {
        if (environmentSfxAudioSource == null)
        {
            Debug.LogWarning("environmentSfxAudioSource is null");
            return;
        }

        if (environmentSfxAudioSource.isPlaying)
            environmentSfxAudioSource.Stop();

        if (winClip != null)
            environmentSfxAudioSource.PlayOneShot(winClip);
    }
}

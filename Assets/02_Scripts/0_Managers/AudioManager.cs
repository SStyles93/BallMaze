using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;
    
    [Header("Music")]
    [SerializeField] private AudioClip musicClip;
    
    [Header("SFX")]
    [SerializeField] private AudioClip jumpClip;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Really not optimal and prone to error but sources should always be defined statically ([SerializeField])
        if (musicAudioSource == null ||sfxAudioSource == null)
        {
            AudioSource[]sources = FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            musicAudioSource = sources[0];
            sfxAudioSource = sources[1];
        }
    }

    private void Start()
    {
        musicAudioSource.clip = musicClip;
        musicAudioSource.Play();
    }

    public void Jump()
    {
        if (sfxAudioSource.isPlaying)
            sfxAudioSource.Stop();
        sfxAudioSource.PlayOneShot(jumpClip);
    }
}

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    [Header("AudioListenerRef")]
    [SerializeField] private GameObject audioListener;
    [SerializeField] private GameObject playerRef;

    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource UiSfxAudioSource;
    [SerializeField] private AudioSource environmentSfxAudioSource;

    [Header("Music")]
    [SerializeField] private AudioClip musicClip;

    [Header("UI SFX")]
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip validateClip;

    [Header("Environment SFX")]
    [SerializeField] private AudioClip starClip;
    [SerializeField] private AudioClip winClip;

    private bool isAudioEnabled = true;
    private bool isMusicEnabled = true;

    public static AudioManager Instance { get; private set; }
    public bool IsAudioEnabled => isAudioEnabled;
    public bool IsMusicEnabled => isMusicEnabled;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (musicAudioSource == null) Debug.LogWarning("MusicAudioSource is null, assign it in inspector");
        if (UiSfxAudioSource == null) Debug.LogWarning("PlayerSfxAudioSource is null, assign it in inspector");
        if (environmentSfxAudioSource == null) Debug.LogWarning("EnvironmentSfxAudioSource is null, assign it in inspector");
    }

    private void Update()
    {
        //if isGame
        if(playerRef != null)
        {
            audioListener.transform.position = playerRef.transform.position;
        }
        else
        {
            audioListener.transform.position = Vector3.zero;
        }
    }

    public void SetPlayer(GameObject player)
    {
        playerRef = player;
    }

    // --- GENERAL ---

    /// <summary>
    /// Sets all the Audio Sources to the given state
    /// </summary>
    /// <param name="isActive">state of the audio sources (On/Off)</param>
    public void SetGeneralAudioState(bool isActive)
    {
        float volume = isActive ? 0.0f : -80.0f;
        mixer.SetFloat("MasterVolume", volume);
        isAudioEnabled = isActive;
    }

    /// <summary>
    /// Sets the Music Source to the given state
    /// </summary>
    /// <param name="isActive"></param>
    public void SetMusicState(bool isActive)
    {
        float volume = isActive ? 0.0f : -80.0f;
        mixer.SetFloat("MusicVolume", volume);
        isMusicEnabled = isActive;
    }

    // --- MUSIC ---
    public void PlayMusic()
    {
        if (musicAudioSource.isPlaying) return;

        if (musicClip != null)
            musicAudioSource.clip = musicClip;

        musicAudioSource.Play();
    }

    // --- PLAYER SFX ---

    public void PlayClickSound()
    {
        UiSfxAudioSource.pitch = 1.0f;
        UiSfxAudioSource.volume = 1.0f;
        PlayUISound(clickClip);
    }

    public void PlayValidate()
    {
        UiSfxAudioSource.pitch = 1.0f;
        UiSfxAudioSource.volume = 1.0f;
        PlayUISound(validateClip);
    }

    // --- ENVIRONMENT SFX ---

    public void PlayStarSound()
    {
        SetEnvironmentPitch();
        PlayEnvironmentSound(starClip);
    }

    public void PlayWinSound()
    {
        SetEnvironmentPitch();
        PlayEnvironmentSound(winClip);
    }

    private void SetEnvironmentPitch()
    {
        LevelManager manager = LevelManager.Instance;
        if (manager == null) return;

        // Star count - 1 (0,1,2)
        // for a Pitch at 0.8 -> 0.9 -> 1.0
        environmentSfxAudioSource.pitch = 0.8f + ((manager.CurrentStarCount - 1) * 0.1f);
    }

    #region Helper Methods

    /// <summary>
    /// Plays a sound on the enviroment Audio Source
    /// </summary>
    /// <param name="clip">The clip to play</param>
    private void PlayEnvironmentSound(AudioClip clip)
    {
        if (environmentSfxAudioSource.isPlaying)
            environmentSfxAudioSource.Stop();

        if (clip == null)
            Debug.LogWarning($"Clip {clip.name} is null");

        environmentSfxAudioSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Plays a sound on the player Audio Source
    /// </summary>
    /// <param name="clip">The clip to play</param>
    private void PlayUISound(AudioClip clip)
    {
        if (UiSfxAudioSource.isPlaying)
            UiSfxAudioSource.Stop();

        if (clip == null)
            Debug.LogWarning($"Clip {clip.name} is null");

        UiSfxAudioSource.PlayOneShot(clip);
    }

    private void SetAudioSourceState(AudioSource audioSource, bool isActive)
    {
        audioSource.enabled = isActive;
    }

    #endregion
}

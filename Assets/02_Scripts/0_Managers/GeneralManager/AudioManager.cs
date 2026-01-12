using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource playerSfxAudioSource;
    [SerializeField] private AudioSource playerRollingAudioSource;
    [SerializeField] private AudioSource environmentSfxAudioSource;

    [Header("Music")]
    [SerializeField] private AudioClip musicClip;

    [Header("Player SFX")]
    [SerializeField] private AudioClip rollingClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip thumpClip;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip validateClip;

    [Header("Environment SFX")]
    [SerializeField] private AudioClip starClip;
    [SerializeField] private AudioClip winClip;

    private bool isAudioEnabled = true;
    private bool isMusicEnabled = true;

    public static AudioManager Instance { get; private set; }
    public AudioSource MusicAudioSource => musicAudioSource;
    public AudioSource PlayerSfxAudioSource => playerSfxAudioSource;
    public AudioSource EnvironmentSfxAudioSource => environmentSfxAudioSource;
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
        if (playerSfxAudioSource == null) Debug.LogWarning("PlayerSfxAudioSource is null, assign it in inspector");
        if (environmentSfxAudioSource == null) Debug.LogWarning("EnvironmentSfxAudioSource is null, assign it in inspector");
        if (playerRollingAudioSource == null) Debug.LogWarning("PlayerRollingAudioSource is null, assign it in inspector");
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


    public void SetRollingVolume(float value)
    {
        if (value <= 0.02f)
        {
            playerRollingAudioSource.Stop();
            playerRollingAudioSource.volume = 0.0f;
        }
        else
        {
            if(!playerRollingAudioSource.isPlaying)
            playerRollingAudioSource.Play();

            playerRollingAudioSource.volume = value;
        }

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

    public void PlayJumpSound()
    {
        playerSfxAudioSource.volume = 1.0f;
        playerSfxAudioSource.pitch = 2.0f;
        PlayPlayerSound(jumpClip);
    }

    public void PlayThumpSound(float pitch)
    {
        playerSfxAudioSource.pitch = pitch;
        playerSfxAudioSource.volume = .3f;
        PlayPlayerSound(thumpClip);
    }

    public void PlayClickSound()
    {
        playerSfxAudioSource.pitch = 1.0f;
        playerSfxAudioSource.volume = 1.0f;
        PlayPlayerSound(clickClip);
    }

    public void PlayValidate()
    {
        playerSfxAudioSource.pitch = 1.0f;
        playerSfxAudioSource.volume = 1.0f;
        PlayPlayerSound(validateClip);
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
    private void PlayPlayerSound(AudioClip clip)
    {
        if (playerSfxAudioSource.isPlaying)
            playerSfxAudioSource.Stop();

        if (clip == null)
            Debug.LogWarning($"Clip {clip.name} is null");

        playerSfxAudioSource.PlayOneShot(clip);
    }

    private void SetAudioSourceState(AudioSource audioSource, bool isActive)
    {
        audioSource.enabled = isActive;
    }

    #endregion
}

using System;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [Header("Player SFX")]
    [SerializeField] private AudioClip rollingClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip thumpClip;
    [SerializeField] private AudioClip fallingClip;

    [Header("Sound Parameters")]
    [SerializeField] private float GroundPitch = 2.8f;
    [SerializeField] private float IcePitch = 5f;

    private Rigidbody rb;
    private PlayerMovement playerMovement;

    // Define the range of expected speeds
    public float maxSpeed = 7.6f;
    // Define the desired range for the audio volume
    public Vector2 volumeRange = new Vector2(0.0f, 1.0f);

    private bool isPlayerGrounded;

    private void OnEnable()
    {
        playerMovement.OnPlayerJumped += PlayJumpSound;
        playerMovement.OnPlayerLanded += PlayLandedSound;
        playerMovement.OnPlayerStateChanged += PlayFallingSound;
    }

    private void OnDisable()
    {
        playerMovement.OnPlayerJumped -= PlayJumpSound;
        playerMovement.OnPlayerLanded -= PlayLandedSound;
        playerMovement.OnPlayerStateChanged -= PlayFallingSound;
    }

    private void Awake()
    {
        rb ??= GetComponent<Rigidbody>();
        playerMovement ??= GetComponent<PlayerMovement>();
        audioSource ??= GetComponent<AudioSource>();
        AudioManager.Instance.SetPlayer(this.gameObject);
    }

    private void FixedUpdate()
    {
        float speed = rb.linearVelocity.magnitude;
        float rollingVolume = speed / maxSpeed;
        if (isPlayerGrounded)
            SetRollingVolume(rollingVolume);
    }

    // --- ROLLING ---
    private void SetRollingVolume(float value)
    {
        if (value <= 0.02f)
        {
            audioSource.Stop();
            audioSource.volume = 0.0f;
        }
        else
        {
            if (!audioSource.isPlaying)
                audioSource.Play();

            audioSource.volume = value;
        }
    }

    // --- JUMP ---

    private void PlayJumpSound()
    {
        isPlayerGrounded = false;
        audioSource.volume = 1.0f;
        audioSource.pitch = 2.0f;
        PlaySoundOnce(jumpClip);
    }

    private void PlayLandedSound(string surfaceTypeTag)
    {
        float pitch = surfaceTypeTag switch
        {
            "Ground" => GroundPitch,
            "Ice" => IcePitch,
            "MovingPlatform" => GroundPitch,
            _ => 1.0f

        };

        PlayThumpSound(pitch);

        isPlayerGrounded = true;
    }

    private void PlayThumpSound(float pitch)
    {
        audioSource.pitch = pitch;
        audioSource.volume = .3f;
        PlaySoundOnce(thumpClip);
    }

    // --- FALL --- 

    private void PlayFallingSound(PlayerState state)
    {
        if(state == PlayerState.IsFalling)
        audioSource.pitch = 1.5f;
        audioSource.volume = 1.0f;
        PlaySoundOnce(fallingClip);
    }

    // --- HELPER ---
    private void PlaySoundOnce(AudioClip clip)
    {
        if (audioSource.isPlaying)
            audioSource.Stop();

        if (clip == null)
            Debug.LogWarning($"Clip {clip.name} is null");

        audioSource.PlayOneShot(clip);
    }
}

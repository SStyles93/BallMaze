using System;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerMovement playerMovement;


    // GET FROM PLAYERMOVEMENT

    // Define the range of expected speeds
    public float maxSpeed = 7.6f;
    // Define the desired range for the audio volume
    public Vector2 volumeRange = new Vector2(0.0f, 1.0f);

    [SerializeField] private float GroundPitch = 2.8f;
    [SerializeField] private float IcePitch = 5f;

    private bool isPlayerGrounded;

    private void OnEnable()
    {
        playerMovement.OnPlayerJumped += PlayJumpSound;
        playerMovement.OnPlayerLanded += PlayLandedSound;
    }

    private void OnDisable()
    {
        playerMovement.OnPlayerJumped -= PlayJumpSound;
        playerMovement.OnPlayerLanded -= PlayLandedSound;
    }

    private void Awake()
    {
        rb ??= GetComponent<Rigidbody>();
        playerMovement ??= GetComponent<PlayerMovement>();
    }


    private void FixedUpdate()
    {
        float speed = rb.linearVelocity.magnitude;
        float rollingVolume = speed / maxSpeed;
        if (isPlayerGrounded)
            AudioManager.Instance.SetRollingVolume(rollingVolume);
    }

    private void PlayJumpSound()
    {
        isPlayerGrounded = false;
        AudioManager.Instance?.PlayJumpSound();
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

        AudioManager.Instance?.PlayThumpSound(pitch);

        isPlayerGrounded = true;
    }
}

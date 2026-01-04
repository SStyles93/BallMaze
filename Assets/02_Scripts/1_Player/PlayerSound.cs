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

    // ^^^^^^^^^^^^^

    private void Awake()
    {
        rb ??= GetComponent<Rigidbody>();
        playerMovement ??= GetComponent<PlayerMovement>();
    }


    private void FixedUpdate()
    {
        float speed = rb.linearVelocity.magnitude;
        float rollingVolume = speed / maxSpeed;
        AudioManager.Instance.SetRollingVolume(rollingVolume);
    }
}

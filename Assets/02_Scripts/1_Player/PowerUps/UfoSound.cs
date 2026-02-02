using UnityEngine;

public class UfoSound : MonoBehaviour
{
    private Rigidbody ufoRigidbody;
    private AudioSource audioSource;

    private void Awake()
    {
        if (!ufoRigidbody)
            ufoRigidbody = GetComponent<Rigidbody>();
        ufoRigidbody.isKinematic = true;

        if (!audioSource)
            audioSource = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        // --- SOUND ---
        float speed = ufoRigidbody.linearVelocity.magnitude;
        float rollingVolume = speed / 8.0f;
        SetVolume(rollingVolume);
    }

    private void SetVolume(float volume)
    {
        if (volume <= 0.5f)
        {
            audioSource.volume = 0.5f;
        }
        else
        {
            if (!audioSource.isPlaying)
                audioSource.Play();

            audioSource.volume = volume;
        }
    }
}

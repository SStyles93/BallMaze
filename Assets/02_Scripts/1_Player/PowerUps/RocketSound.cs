using UnityEngine;

public class RocketSound : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;

    [SerializeField] AudioClip rumbleSound;
    [SerializeField] AudioClip burstSound;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayRumbleSound()
    {
        if (audioSource != null && rumbleSound != null)
        {
            if (audioSource.isPlaying) audioSource.Stop();
            audioSource.clip = rumbleSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        //Debug.Log("RocketSound - Rumble");
    }

    public void PlayBurstSound()
    {
        if (audioSource != null && rumbleSound != null)
        {
            if (audioSource.isPlaying) audioSource.Stop();
            audioSource.loop = false;
            audioSource.PlayOneShot(burstSound);
        }
        //Debug.Log("RocketSound - Burst");
    }
}

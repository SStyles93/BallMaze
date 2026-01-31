using UnityEngine;

[RequireComponent(typeof(PlatformMovement))]
public class MovingPlatformAudio : MonoBehaviour
{
    [SerializeField] private PlatformMovement platformMovement;
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        platformMovement ??= GetComponent<PlatformMovement>();
        audioSource ??= GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        platformMovement.OnPlatformActive += SetMovementSound;
    }

    private void OnDisable()
    {
        platformMovement.OnPlatformActive -= SetMovementSound;
    }

    public void SetMovementSound(bool isMoving)
    {
        if (!isMoving)
        {
            audioSource.Stop();
            audioSource.volume = 0.0f;
        }
        else
        {
            if (!audioSource.isPlaying)
                audioSource.Play();

            audioSource.volume = 1.0f;
        }
    }
}

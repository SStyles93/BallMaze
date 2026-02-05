using UnityEngine;
using DG.Tweening;

public class EndTrigger : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float animationDuration = 3f;

    [Header("Levitation")]
    [SerializeField] private float levitationHeight = 2f;
    [Range(0f, 1f)]
    [SerializeField] private float levitationDistribution = 0.15f;

    [Header("Burst")]
    [SerializeField] private float burstForce = 200f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip rumbleClip;
    [SerializeField] private float rumblePitch = 1.0f;
    [SerializeField] private AudioClip cometClip;
    [SerializeField] private float cometPitch = 2.0f;

    private bool wasLevelProcessed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || wasLevelProcessed)
            return;

        wasLevelProcessed = true;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        PlayEndAnimation(other.transform, rb);
    }

    private void PlayEndAnimation(Transform player, Rigidbody rb)
    {
        // Global timing
        float centerTime = animationDuration * 0.10f;
        float levitateTime = animationDuration * 0.60f;
        float burstTime = animationDuration * 0.30f;

        // Levitation sub-phases
        float alignTime = levitateTime * levitationDistribution;
        float spinTime = levitateTime * (1- levitationDistribution);

        Vector3 centerPosition = new Vector3(
            transform.position.x,
            player.position.y,
            transform.position.z
        );

        Sequence seq = DOTween.Sequence();

        // Move to center
        seq.Append(
            player.DOMove(centerPosition, centerTime)
                  .SetEase(Ease.InOutQuad)
        );

        // Levitation (full duration)
        seq.Append(
            player.DOMoveY(
                player.position.y + levitationHeight,
                levitateTime
            ).SetEase(Ease.OutSine)
        );

        float levitationStart = seq.Duration() - levitateTime;

        // Smooth re-orient (X/Z → 0)
        Vector3 startEuler = player.rotation.eulerAngles;

        seq.Insert(
            levitationStart,
            DOTween.To(
                () => startEuler,
                v =>
                {
                    startEuler = v;
                    player.rotation = Quaternion.Euler(
                        v.x,
                        player.rotation.eulerAngles.y,
                        v.z
                    );
                },
                new Vector3(0f, startEuler.y, 0f),
                alignTime
            ).SetEase(Ease.OutQuad)
        );

        // Spin
        seq.Insert(
            levitationStart + alignTime,
            player.DORotate(
                new Vector3(0f, 360f * 6f, 0f),
                spinTime,
                RotateMode.FastBeyond360
            ).SetEase(Ease.InQuad)
        );

        // RUMBLE + SHAKE (spin only)
        seq.InsertCallback(
            levitationStart + alignTime,
            () =>
            {
                PlayerCamera.Shake(spinTime, 1.2f);

                if (audioSource && rumbleClip)
                {
                    audioSource.pitch = rumblePitch;
                    audioSource.clip = rumbleClip;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
        );

        // Stop rumble exactly when spin ends
        seq.InsertCallback(
            levitationStart + alignTime + spinTime,
            () =>
            {
                if (audioSource && audioSource.isPlaying)
                    audioSource.Stop();
            }
        );

        // Burst
        seq.AppendCallback(() =>
        {
            player.DOKill(false);

            PlayerCamera.SetCameraFollow(null);

            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(Vector3.up * burstForce, ForceMode.Impulse);

            // 🔊 COMET
            if (audioSource && cometClip)
            {
                audioSource.pitch = cometPitch;
                audioSource.loop = false;
                audioSource.PlayOneShot(cometClip);
            }
        });

        // Spin continues during burst
        seq.Append(
            player.DORotate(
                new Vector3(0f, 360f * 10f, 0f),
                burstTime,
                RotateMode.FastBeyond360
            ).SetEase(Ease.Linear)
        );

        // Scene transition
        seq.OnComplete(() =>
        {
            LevelManager.Instance.ProcessLevelData();
            SavingManager.Instance.SaveSession();
            SceneController.Instance.NewTransition()
                .Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.EndPannel)
                .Perform();

            rb.isKinematic = true;
        });
    }
}

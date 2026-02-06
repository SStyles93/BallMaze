using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Start()
    {
        wasLevelProcessed = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || wasLevelProcessed)
            return;

        wasLevelProcessed = true;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        StartCoroutine(EndSequence(other.transform, rb));
    }

    private IEnumerator EndSequence(Transform player, Rigidbody rb)
    {
        float centerTime = animationDuration * 0.10f;
        float levitateTime = animationDuration * 0.60f;
        float burstTime = animationDuration * 0.30f;

        float alignTime = levitateTime * levitationDistribution;
        float spinTime = levitateTime * (1f - levitationDistribution);

        Vector3 startPos = player.position;
        Vector3 centerPos = new Vector3(
            transform.position.x,
            player.position.y,
            transform.position.z
        );

        // ───────────────────────
        // Move to center
        // ───────────────────────
        yield return MoveOverTime(player, startPos, centerPos, centerTime, EaseInOut);

        // ───────────────────────
        // Levitate
        // ───────────────────────
        Vector3 levitateStart = player.position;
        Vector3 levitateEnd = levitateStart + Vector3.up * levitationHeight;

        StartCoroutine(RotateToUpright(player, alignTime));
        StartCoroutine(Spin(player, spinTime, 6f));

        StartRumble(spinTime);

        yield return MoveOverTime(player, levitateStart, levitateEnd, levitateTime, EaseOutSine);

        StopRumble();

        // ───────────────────────
        // Burst
        // ───────────────────────
        PlayerCamera.SetCameraFollow(null);

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(Vector3.up * burstForce, ForceMode.Impulse);

        PlayCometSound();

        yield return Spin(player, burstTime, 10f);

        // ───────────────────────
        // Scene transition
        // ───────────────────────
        rb.isKinematic = true;

        LevelManager.Instance.ProcessLevelData();
        SavingManager.Instance.SaveSession();

        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.EndPannel)
            .Perform();
    }

    // --- HELPER FUNCTIONS ---

    private IEnumerator MoveOverTime(
    Transform t,
    Vector3 from,
    Vector3 to,
    float duration,
    System.Func<float, float> ease)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float tNorm = elapsed / duration;
            t.position = Vector3.Lerp(from, to, ease(tNorm));
            elapsed += Time.deltaTime;
            yield return null;
        }

        t.position = to;
    }

    private IEnumerator RotateToUpright(Transform t, float duration)
    {
        Quaternion start = t.rotation;
        Quaternion end = Quaternion.Euler(0f, t.eulerAngles.y, 0f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            t.rotation = Quaternion.Slerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        t.rotation = end;
    }

    private IEnumerator Spin(Transform t, float duration, float turns)
    {
        float speed = 360f * turns / duration;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            t.Rotate(Vector3.up, speed * Time.deltaTime, Space.World);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }


    // --- Audio Helpers

    private void StartRumble(float duration)
    {
        PlayerCamera.Shake(duration, 1.2f);

        if (!audioSource || !rumbleClip) return;

        audioSource.pitch = rumblePitch;
        audioSource.clip = rumbleClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void StopRumble()
    {
        if (audioSource && audioSource.isPlaying)
            audioSource.Stop();
    }

    private void PlayCometSound()
    {
        if (!audioSource || !cometClip) return;

        audioSource.pitch = cometPitch;
        audioSource.loop = false;
        audioSource.PlayOneShot(cometClip);
    }


    // --- Easing functions

    private float EaseInOut(float t)
    {
        return t * t * (3f - 2f * t);
    }

    private float EaseOutSine(float t)
    {
        return Mathf.Sin(t * Mathf.PI * 0.5f);
    }

}

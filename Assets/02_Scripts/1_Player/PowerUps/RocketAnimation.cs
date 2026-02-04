using UnityEngine;
using DG.Tweening;
using System;

public class RocketAnimation : MonoBehaviour
{
    [SerializeField] private PhysicalMazeGenerator physicalMazeGeneratorRef;
    [SerializeField] private RocketSound rocketSound;
    [SerializeField] private ParticleSystem ps;

    [Header("Animation Ratios")]
    [SerializeField, Range(0f, 1f)]
    private float rotatePhaseRatio = 0.6f;

    [Header("Shake")]
    [SerializeField] private float shakeStrength = 0.3f;
    [SerializeField] private int shakeVibrato = 20;

    [Header("Burst")]
    [SerializeField] private Ease burstEase = Ease.OutCubic;
    [SerializeField] private float burstEasePower = 2f;

    private float totalDuration;
    private Vector3 targetWorldPos;
    private Tween sequence;

    private void OnEnable()
    {
        if (physicalMazeGeneratorRef == null)
        {
            Debug.Log("No Physical Maze Generator assigned");
            return;
        }
        if (rocketSound == null) rocketSound = GetComponent<RocketSound>();

        targetWorldPos = physicalMazeGeneratorRef.GetEndNeighbourPosition();
        targetWorldPos.y = PowerUpManager.Instance.GetPowerUpHeightOffset(CoinType.ROCKET);

        // Pull duration directly from PowerUpManager
        totalDuration = PowerUpManager.Instance
            .GetPowerUpDuration(CoinType.ROCKET);

        PlaySequence();
    }

    private void OnDisable()
    {
        sequence?.Kill();
    }

    private void PlaySequence()
    {
        float rotateDuration = totalDuration * rotatePhaseRatio;
        float burstDuration = (totalDuration - 0.5f) * (1f - rotatePhaseRatio);

        Camera cam = Camera.main;

        // 1. Calculate the direction from the rocket to the target
        Vector3 dirToTarget = (targetWorldPos - transform.position).normalized;

        // 2. Calculate the target rotation.
        // We want the rocket's local 'up' (the tip) to point at 'dirToTarget'.
        // We start from the current rotation and apply the difference.
        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, dirToTarget);

        Sequence seq = DOTween.Sequence();

        seq.InsertCallback(0, () =>
        {
            PlayerCamera.Shake(rotateDuration, shakeStrength);
            VibrationManager.Instance.Classic();
            rocketSound.PlayRumbleSound();
            ps.Play();
        });

        // rocket shake (charge phase)
        seq.Join(
            transform.DOShakePosition(
                rotateDuration,
                shakeStrength,
                shakeVibrato,
                90f,
                false,
                true
            )
        );

        // Rotation: Rotate the rocket so the tip (Up) faces the target
        seq.Join(
            transform.DORotateQuaternion(targetRotation, rotateDuration)
                .SetEase(Ease.InOutSine)
        );

        seq.AppendCallback(() =>
        {
            rocketSound.PlayBurstSound();
            ps.Stop();
        });

        // Burst (movement phase)
        seq.Append(
            transform.DOMove(targetWorldPos, burstDuration)
                .SetEase(burstEase, burstEasePower)
        );

        sequence = seq;
    }
}

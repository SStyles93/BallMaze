using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RocketAnimation : MonoBehaviour
{
    [SerializeField] private PhysicalMazeGenerator physicalMazeGeneratorRef;
    [SerializeField] private PowerUpManager powerUpManagerRef;

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
    [SerializeField] private Vector3 targetWorldPos;
    private Sequence sequence;

    private void OnEnable()
    {
        if (physicalMazeGeneratorRef == null)
        {
            Debug.Log("No Physical Maze Generator assigned");
            return;
        }

        if (powerUpManagerRef == null) powerUpManagerRef = transform.GetComponentInParent<PowerUpManager>();

        if (rocketSound == null) rocketSound = GetComponent<RocketSound>();

        if (!physicalMazeGeneratorRef.TryGetWalkableEndNeighbours(out List<Vector3> worldPositions))
        {
            Debug.Log("No walkable neighbour");
        }

        if (!physicalMazeGeneratorRef.IsGridGenerated)
            targetWorldPos = worldPositions[Random.Range(0, worldPositions.Count)];

        targetWorldPos.y = powerUpManagerRef.GetPowerUpHeightOffset(CoinType.ROCKET);

        // Pull duration directly from PowerUpManager
        totalDuration = powerUpManagerRef
            .GetPowerUpDuration(CoinType.ROCKET);

        PlaySequence();
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

        sequence?.Kill();
        sequence = DOTween.Sequence()
            .SetTarget(this)
            .SetLink(gameObject, LinkBehaviour.KillOnDisable);

        sequence.InsertCallback(0, () =>
        {
            PlayerCamera.Shake(rotateDuration, shakeStrength);
            VibrationManager.Instance.Classic();
            rocketSound.PlayRumbleSound();
            ps.Play();
        });

        // rocket shake (charge phase)
        sequence.Join(
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
        sequence.Join(
            transform.DORotateQuaternion(targetRotation, rotateDuration)
                .SetEase(Ease.InOutSine)
        );

        sequence.AppendCallback(() =>
        {
            rocketSound.PlayBurstSound();
            ps.Stop();
        });

        // Burst (movement phase)
        sequence.Append(
            transform.DOMove(targetWorldPos, burstDuration)
                .SetEase(burstEase, burstEasePower)
        );
    }

    private void OnDisable()
    {
        DOTween.Kill(this); // kills only tweens owned by this RocketAnimation
    }

}

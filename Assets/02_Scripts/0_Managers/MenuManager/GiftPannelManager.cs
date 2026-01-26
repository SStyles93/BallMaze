using DG.Tweening;
using TMPro;
using UnityEngine;

public class GiftPannelManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject giftObject;
    [SerializeField] private GameObject coinObject;

    [Header("Animation Settings")]
    [SerializeField] private float shakeInterval = 3f;
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeStrength = 15f;

    [Header("Gift Value")]
    [SerializeField] private int giftValue = 450;

    private bool giftOpened = false;
    private bool canClick = true;

    private Sequence shakeSequence;

    private void Start()
    {
        coinObject.SetActive(false);

        coinObject.GetComponentInChildren<TMP_Text>().text = $"<sprite index=0> {giftValue}";

        StartGiftShake();
    }

    public void OnPannelClicked()
    {
        if (!canClick)
            return;

        if (!giftOpened)
            OpenGift();
        else
            UnloadGiftPannel();
    }

    #region Gift Shake

    private void StartGiftShake()
    {
        shakeSequence = DOTween.Sequence()
            .SetLoops(-1)
            .AppendInterval(shakeInterval)
            .Append(
                giftObject.transform
                    .DOShakeRotation(
                        shakeDuration,
                        new Vector3(0, 0, shakeStrength),
                        vibrato: 20,
                        randomness: 90,
                        fadeOut: true
                    )
            );
    }

    private void StopGiftShake()
    {
        if (shakeSequence != null && shakeSequence.IsActive())
            shakeSequence.Kill();
    }

    #endregion

    #region Gift Open

    private void OpenGift()
    {
        canClick = false;
        giftOpened = true;

        StopGiftShake();

        Sequence openSequence = DOTween.Sequence();

        openSequence.Append(
            giftObject.transform
                .DOScale(0f, 0.35f)
                .SetEase(Ease.InBack)
        );

        openSequence.AppendCallback(() =>
        {
            giftObject.SetActive(false);
            coinObject.SetActive(true);
            coinObject.transform.localScale = Vector3.zero;
        });

        openSequence.Append(
            coinObject.transform
                .DOScale(1f, 0.4f)
                .SetEase(Ease.OutBack)
        );

        openSequence.OnComplete(() =>
        {
            canClick = true;
        });
    }

    #endregion

    #region Panel Unload

    private void UnloadGiftPannel()
    {
        canClick = false;

        coinObject.transform
            .DOScale(0f, 0.25f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                CoinManager.Instance.IncreaseCurrencyAmount(CoinType.COIN, giftValue);
                CoinManager.Instance.HasPlayerReceivedGift = true;

                SavingManager.Instance.SavePlayer();

                SceneController.Instance
                    .NewTransition()
                    .Unload(SceneDatabase.Scenes.GiftPannel)
                    .SetActive(SceneController.Instance.PreviousActiveScene)
                    .Perform();
            });
    }

    #endregion
}

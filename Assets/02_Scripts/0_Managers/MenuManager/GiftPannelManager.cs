using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class GiftDefinition
{
    [Header("Gift Type")]
    public CoinType type;
    [Header("Gift Value")]
    public int value = 0;
}

public class GiftPannelManager : MonoBehaviour
{
    public static GiftPannelManager Instance;

    [Header("Scene References")]
    [SerializeField] private GameObject giftPannel;
    [SerializeField] private GameObject giftBoxObject;
    [SerializeField] private GameObject giftContentLayout;

    [Header("Project Object References")]
    [SerializeField] private GameObject giftObjectPrefab;
    [SerializeField] private Sprite[] giftSprites = new Sprite[5];

    [Header("Animation Settings")]
    [SerializeField] private float shakeInterval = 3f;
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeStrength = 15f;
    [SerializeField] private Ease openEase = Ease.OutBack;
    [SerializeField] private Ease closeEase = Ease.InBack;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip giftSound;
    [SerializeField] private AudioClip glitterSound;

    private GiftPannelPlan currentGiftPannelPlan;
    private List<GameObject> currentGiftObjects = new List<GameObject>();
    private Sequence shakeSequence;
    private bool giftOpened = false;
    private bool canClick = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        bool planMustBePerformed = false;

        GiftPannelPlan plan = GiftPannelManager.Instance.NewPlan();

        plan.WithShake()
            .WithDelay(1);

        if (!CoinManager.Instance.wasCoinsReceived)
        {
            plan.AddGift(CoinType.COIN, 450);
            CoinManager.Instance.wasCoinsReceived = true;
            planMustBePerformed = true;
        }
        if (!CoinManager.Instance.wasRocketReceived && LevelManager.Instance.GetHighestFinishedLevelIndex() > 9)
        {
            plan.AddGift(CoinType.ROCKET, 5);
            CoinManager.Instance.wasRocketReceived = true;
            planMustBePerformed = true;
        }
        if (!CoinManager.Instance.wasUfoReceived && LevelManager.Instance.GetHighestFinishedLevelIndex() > 19)
        {
            plan.AddGift(CoinType.UFO, 5);
            CoinManager.Instance.wasUfoReceived = true;
            planMustBePerformed = true;
        }

        if (planMustBePerformed)
        {
            plan.Perform();
        }
    }

    public void OnClickGiftBox()
    {
        if (!canClick) return;

        canClick = false;

        if (!giftOpened)
        {
            // Open the gifts
            StartCoroutine(OpenGiftRoutine(currentGiftObjects));
        }
        else
        {
            // Close the panel
            CloseGiftPanel(currentGiftPannelPlan);
        }
    }


    #region GiftPlan

    public GiftPannelPlan NewPlan()
    {
        return new GiftPannelPlan();
    }
    private IEnumerator ExecutePlan(GiftPannelPlan plan)
    {
        canClick = false;
        giftOpened = false;

        giftBoxObject.SetActive(true);
        giftBoxObject.transform.localScale = Vector3.one;

        currentGiftPannelPlan = plan;
        currentGiftObjects = new List<GameObject>();

        // Initialize gift objects
        foreach (var gift in plan.Gifts)
        {
            GameObject obj = Instantiate(giftObjectPrefab, giftContentLayout.transform);
            obj.SetActive(false);

            TMP_Text text = obj.GetComponentInChildren<TMP_Text>();
            if (gift.type == CoinType.COIN)
                text.text = $"<sprite index=0> {gift.value}";
            else
                text.text = $"x{gift.value}";

            obj.GetComponentInChildren<Image>().sprite = giftSprites[(int)gift.type];

            currentGiftObjects.Add(obj);
        }

        if (plan.Delay > 0)
        {
            yield return new WaitForSeconds(plan.Delay);
        }

        giftPannel.SetActive(true);
        audioSource.PlayOneShot(giftSound);

        // Start shake animation if requested
        if (plan.Shake)
        {
            StartGiftShake();
        }

        // Wait one frame before opening
        yield return null;

        if (plan.AutoOpenFlag)
        {
            yield return OpenGiftRoutine(currentGiftObjects);
        }
        else
        {
            canClick = true;
        }
    }
    private void StartGiftShake()
    {
        shakeSequence = DOTween.Sequence()
            .SetLoops(-1)
            .AppendInterval(shakeInterval)
            .Append(
                giftBoxObject.transform
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
    private IEnumerator OpenGiftRoutine(List<GameObject> giftObjects)
    {
        StopGiftShake();

        // Play glitter sound
        audioSource.clip = glitterSound;
        audioSource.loop = true;
        audioSource.Play();

        // Scale down gift box
        yield return giftBoxObject.transform
            .DOScale(0f, 0.35f)
            .SetEase(Ease.InBack)
            .WaitForCompletion();

        giftBoxObject.SetActive(false);

        // Show gifts one by one
        foreach (var giftObj in giftObjects)
        {
            giftObj.SetActive(true);
            giftObj.transform.localScale = Vector3.zero;
            yield return giftObj.transform
                .DOScale(1f, 0.4f)
                .SetEase(openEase)
                .WaitForCompletion();
        }

        giftOpened = true;
        canClick = true;
    }
    private void CloseGiftPanel(GiftPannelPlan plan)
    {
        if (!giftOpened) return;


        canClick = false;
        audioSource.Stop();

        // Assuming gifts are child objects of parent
        List<Transform> gifts = new();
        foreach (Transform child in giftContentLayout.transform)
        {
            if (child != giftBoxObject)
                gifts.Add(child);
        }

        Sequence seq = DOTween.Sequence();
        foreach (var g in gifts)
        {
            seq.Join(g.DOScale(0f, 0.25f).SetEase(closeEase));
        }

        seq.OnComplete(() =>
        {
            foreach (var giftDef in currentGiftPannelPlan.Gifts)
            {
                CoinManager.Instance.IncreaseCurrencyAmount(giftDef.type, giftDef.value);
            }
            SavingManager.Instance.SavePlayer();

            foreach (var giftObject in gifts)
            {
                Destroy(giftObject.gameObject);
            }
            canClick = true;
            giftOpened = false;
            giftPannel.SetActive(false);
        });
    }

    // --- Plan class ---
    public class GiftPannelPlan
    {
        private List<GiftDefinition> gifts = new();
        private bool shake = false;
        private bool autoOpen = false;
        private float delay;

        public GiftPannelPlan AddGift(CoinType type, int value)
        {
            gifts.Add(new GiftDefinition { type = type, value = value });
            return this;
        }

        public GiftPannelPlan WithShake()
        {
            shake = true;
            return this;
        }
        public GiftPannelPlan WithDelay(float delay)
        {
            this.delay = delay;
            return this;
        }

        public GiftPannelPlan AutoOpen()
        {
            autoOpen = true;
            return this;
        }

        public void Perform()
        {
            GiftPannelManager.Instance.StartCoroutine(
                GiftPannelManager.Instance.ExecutePlan(this)
            );
        }

        internal List<GiftDefinition> Gifts => gifts;
        internal bool Shake => shake;
        internal bool AutoOpenFlag => autoOpen;

        internal float Delay => delay;
    }

    #endregion

}

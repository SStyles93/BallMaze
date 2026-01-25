using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ValidationPannelManager : MonoBehaviour
{
    [Header("Player Data")]
    [SerializeField] private PlayerSkinData_SO playerSkinData_SO;

    [Header("Validation Pannel")]
    [SerializeField] private GameObject validationPannel;
    [SerializeField] private TMP_Text validationText;
    [SerializeField] private GameObject buyButton;
    [SerializeField] private TMP_Text buyButtonText = null;
    [SerializeField] private GameObject lockImage;
    [SerializeField] private TMP_Text lockText = null;

    [Header("Insufficient Funds Pannel")]
    [SerializeField] private GameObject insufficientFundsPannel;


    [SerializeField] private CustomizationOption selectedOption = null;

    private void OnEnable()
    {
        CustomizationManager.Instance.OnOptionChanged += SetSelectedOption;
    }
    private void OnDisable()
    {
        CustomizationManager.Instance.OnOptionChanged -= SetSelectedOption;
    }

    private void Awake()
    {
        if (buyButton != null && buyButtonText == null)
            buyButtonText = buyButton.GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        validationPannel.SetActive(false);
        insufficientFundsPannel.SetActive(false);
        buyButton.SetActive(false);
    }

    /// <summary>
    /// Opens and Initializes the Validation Pannel with the selected Option
    /// </summary>
    /// <remarks>Method called from the "BUY" button in the customization scene</remarks>
    public void OpenValidationPannel()
    {
        validationPannel.SetActive(true);
        InitializePannelWithOption(selectedOption);
    }

    /// <summary>
    /// Method called by pannel Button
    /// </summary>
    public void ClosePannel()
    {
        validationPannel.SetActive(false);
    }

    /// <summary>
    /// Tryies to validate the current purchase
    /// </summary>
    public void ValidatePurchase()
    {
        if (CustomizationManager.Instance.ValidatePurchase())
        {
            buyButton.gameObject.SetActive(false);
            validationPannel.SetActive(false);
        }
        else
        {
            insufficientFundsPannel.SetActive(true);
            insufficientFundsPannel.GetComponent<InsufficientFundsPannelManager>().InitializePannel(selectedOption.price.CoinType);
            validationPannel.SetActive(false);
        }
    }

    // --- PRIVATE METHODS ---

    private void InitializePannelWithOption(CustomizationOption option)
    {
        selectedOption = option;
        InitializeText(option);
    }

    private void InitializeText(CustomizationOption option)
    {
        validationText.text = $"Purchase for <sprite index={(int)option.price.CoinType}> {option.price.Amount} ?";
    }

    private void SetSelectedOption(CustomizationSlot slot)
    {
        selectedOption = slot.option;
        if (slot.option.isLocked)
        {
            buyButton.SetActive(true);
            SetBuyButtonText(selectedOption);

            // NOT LOCKED BY LEVEL
            if (!slot.isLockedByLevel)
            {
                SetBuyButtonActive(true);
                lockImage.SetActive(false);
            }
            else
            {
                SetLockText(selectedOption);
                lockImage.SetActive(true);
                SetBuyButtonActive(false);
            }
        }
        else
        {
            buyButton.SetActive(false);
            lockImage.SetActive(false);
        }
    }

    private void SetBuyButtonActive(bool isBuyActive)
    {
        if (isBuyActive)
        {
            
            buyButton.GetComponent<Button>().interactable = true;
            buyButton.SetImageAlphaValue(255);
            buyButtonText.SetTextAlphaValue(255);
        }
        else
        {
            buyButton.GetComponent<Button>().interactable = false;
            buyButton.SetImageAlphaValue(50);
            buyButtonText.SetTextAlphaValue(50);
        }
    }

    private void SetBuyButtonText(CustomizationOption option)
    {
        buyButtonText.text = $"<sprite index={(int)option.price.CoinType}> {option.price.Amount}";
    }

    private void SetLockText(CustomizationOption option)
    {
        lockText.text = $"Unlock at level {option.levelToUnlock}";
    }
}

public static class GameObjectExtensions
{
    public static void SetImageAlphaValue(this GameObject gameObject, int alphaValue)
    {
        Image image = gameObject.GetComponent<Image>();
        if (image == null) return;

        Color tintedColor = image.color;
        tintedColor.a = 1.0f / 255.0f * alphaValue;
        image.color = tintedColor;
    }

    public static void SetTextAlphaValue(this TMP_Text text, int alphaValue)
    {
        Color tintedColor = text.color;
        tintedColor.a = 1.0f/255.0f * alphaValue;
        text.color = tintedColor;
    }
}

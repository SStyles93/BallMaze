using TMPro;
using UnityEngine;

public class ValidationPannelManager : MonoBehaviour
{
    [Header("Player Data")]
    [SerializeField] private PlayerSkinData_SO playerSkinData_SO;

    [Header("Validation Pannel")]
    [SerializeField] private GameObject validationPannel;
    [SerializeField] private GameObject buyButton;
    [SerializeField] private TMP_Text buyButtonText = null;
    [SerializeField] private TMP_Text validationText;

    [Header("Insufficient Funds Pannel")]
    [SerializeField] private GameObject insufficientFundsPannel;

    [SerializeField] private GameObject colorTab;

    private CustomizationOption selectedOption = null;

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
        if(buyButton != null && buyButtonText == null)
            buyButtonText = buyButton.GetComponent<TMP_Text>();
    }

    private void Start()
    {
        validationPannel.SetActive(false);
        insufficientFundsPannel.SetActive(false);
        buyButton.SetActive(false);

        bool showTab = !playerSkinData_SO.skinOption.isPremium;
        colorTab.SetActive(showTab);
    }

    /// <summary>
    /// Opens and Initializes the Validation Pannel with the selected Option
    /// </summary>
    /// <remarks>Method called from the "BUY" button in the customization scene</remarks>
    public void OnBuyButtonClicked()
    {
        validationPannel.SetActive(true);
        InitializePannelWithOption(selectedOption);
    }

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
            // Disables the Buy button
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
        SetBuyButtonText(slot);
        bool showTab = !playerSkinData_SO.skinOption.isPremium;
        colorTab.SetActive(showTab);
    }

    private void SetBuyButtonText(CustomizationSlot slot)
    {
        // Enable the button if locked (not bought) and not locked by level
        bool showBuyButton = slot.option.isLocked && !slot.isLockedByLevel;

        buyButton.gameObject.SetActive(showBuyButton);
        buyButtonText.text = $"<sprite index={(int)slot.option.price.CoinType}> {selectedOption.price.Amount}";
    }
}

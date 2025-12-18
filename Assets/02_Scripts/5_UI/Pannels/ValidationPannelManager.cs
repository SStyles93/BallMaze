using System;
using TMPro;
using UnityEngine;

public class ValidationPannelManager : MonoBehaviour
{
    [Header("Validation Pannel")]
    [SerializeField] private GameObject validationPannel;
    [SerializeField] private GameObject buyButton;
    [SerializeField] private TMP_Text buyButtonText = null;
    [SerializeField] private TMP_Text validationText;

    [Header("Insufficient Funds Pannel")]
    [SerializeField] private GameObject insufficientFundsPannel;


    [SerializeField] private CustomizationOption selectedOption = null;
    private GameObject optionObject;

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

    /// <summary>
    /// Tryies to validate the current purchase
    /// </summary>
    public void ValidatePurchase()
    {
        if (CustomizationManager.Instance.ValidatePurchase())
        {
            // Close pannel and reset pannel values when purchase is successful
            ClosePannel();
            // Disables the Buy button
            buyButton.gameObject.SetActive(false);

        }
        else
        {
            ClosePannel();
            insufficientFundsPannel.SetActive(true);
        }
    }

    // --- PRIVATE METHODS ---
    private void ClosePannel()
    {
        validationPannel.SetActive(false);
        selectedOption = null;
        optionObject = null;
    }

    private void InitializePannelWithOption(CustomizationOption option)
    {
        selectedOption = option;

       //TODO: OptionObject has to be passed here

        InitializeText(option);
    }

    private void InitializeText(CustomizationOption option)
    {
        validationText.text = $"Purchase for {option.price} <sprite index=0> ?";
    }

    private void SetSelectedOption(CustomizationOption option)
    {
        selectedOption = option;
        SetBuyButtonText(option);
    }

    private void SetBuyButtonText(CustomizationOption option)
    {
        // Enable the button if locked
        buyButton.gameObject.SetActive(option.isLocked);
        buyButtonText.text = $"{selectedOption.price} <sprite index=0>";
    }
}

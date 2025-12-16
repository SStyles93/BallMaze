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
    private string optionName;

    private void OnEnable()
    {
        ShopManager.Instance.OnOptionChanged += SetSelectedOption;
    }
    private void OnDisable()
    {
        ShopManager.Instance.OnOptionChanged -= SetSelectedOption;
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
    /// Initializes the Validation Pannel with the selected Option
    /// </summary>
    /// <remarks>Method called from the "BUY" button in the customization scene</remarks>
    public void OnBuyButtonClicked()
    {
        InitializePannelWithOption(selectedOption);
    }

    public void ValidatePurchase()
    {
        if (ShopManager.Instance.ValidatePurchase())
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
        optionName = null;
    }

    private void InitializePannelWithOption(CustomizationOption option)
    {
        validationPannel.SetActive(true);
        selectedOption = option;

        if (option is ColorOption col)
        {
            optionName = col.name;
        }
        if (option is MaterialOption mat)
        {
            optionName = mat.material.name;
        }

        InitializeText();
    }

    private void InitializeText()
    {
        validationText.text = $"Purchase {optionName} for {selectedOption.price} <sprite index=0> ?";
    }

    private void SetSelectedOption(CustomizationOption option)
    {
        selectedOption = option;
        SetBuyButton(option);
    }

    private void SetBuyButton(CustomizationOption option)
    {
        // Enable the button if locked
        buyButton.gameObject.SetActive(option.isLocked);
        buyButtonText.text = $"{selectedOption.price} <sprite index=0>";
    }
}

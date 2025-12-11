using System;
using TMPro;
using UnityEngine;

public class ValidationPannelManager : MonoBehaviour
{
    [SerializeField] private GameObject validationPannel;
    [SerializeField] private TMP_Text validationText;

    [SerializeField] private CustomizationOption selectedOption = null;
    private string optionName;

    private void OnEnable()
    {
        ShopManager.Instance.OnOptionChanged += InitializePannelWithOption;
    }

    private void OnDisable()
    {
        ShopManager.Instance.OnOptionChanged -= InitializePannelWithOption;
    }

    private void Start()
    {
        validationPannel.SetActive(false);
    }

    public void InitializePannelWithOption(CustomizationOption option)
    {
        validationPannel.SetActive(true);
        selectedOption = option;

        if(option is ColorOption col)
        {
            optionName = col.name;
        }
        if(option is MaterialOption mat)
        {
            optionName = mat.material.name;
        }

        InitializeText();
    }

    public void ClosePannel()
    {
        validationPannel.SetActive(false);
        selectedOption = null;
        optionName = null;
    }

    public void ValidatePurchase()
    {
        if (ShopManager.Instance.ValidatePurchase())
        {
            // Close pannel and reset pannel values when purchase is successful
            validationPannel.SetActive(false);
            selectedOption = null;
            optionName = null;
        }
        else
        {
            validationPannel.SetActive(false);
            selectedOption = null;
            optionName = null;

            // Activate an "Insufficient funds" pannel
        }
    }

    private void InitializeText()
    {
        validationText.text = $"Purchase {optionName} for {selectedOption.price} <sprite index=0> ?";
    }
}

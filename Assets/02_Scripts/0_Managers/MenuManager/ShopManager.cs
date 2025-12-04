using System;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    public CustomizationData_SO customizationData_SO;
    public PlayerSkinData_SO skinData_SO;

    private CustomizationSlot currentSlot = null;
    private CustomizationOption currentOption = null;
    private int currentOptionIndex = 0;

    public event Action<CustomizationOption> OnOptionChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    public void SetCurrentCustomizationSlot(CustomizationSlot slot)
    {
        currentSlot = slot;
        currentOption = slot.option;
        currentOptionIndex = slot.index;
        // This will call the validation pannel to be initialized with the current option
        OnOptionChanged?.Invoke(currentOption);
    }

    public bool ValidatePurchase()
    {
        if(CurrencyManager.Instance.CurrencyValue < currentOption.price)
        {
            return false;
        }
        else
        {
            DeductPurchaseFromCurrency(currentOption.price);
            UnlockOption(currentOption, currentOptionIndex);

            // TODO: Validation (V+S FX)

            UpdatePlayerOption(currentOption);

            return true;
        }
    }

    private void DeductPurchaseFromCurrency(int price)
    {
        CurrencyManager.Instance.ReduceCurrency(price);
    }

    /// <summary>
    /// Unlocks the customization option in the Data_SO and Slot
    /// </summary>
    /// <param name="option"></param>
    /// <param name="optionIndex"></param>
    private void UnlockOption(CustomizationOption option, int optionIndex)
    {
        if(option is ColorOption colorOption)
        {
            customizationData_SO.colors[optionIndex].isLocked = false;
        }
        if(option is MaterialOption matOption)
        {
            customizationData_SO.materials[optionIndex].isLocked = false;
        }
        currentSlot.Unlock();
    }
    
    /// <summary>
    /// Updates the SkinData_SO with the given option
    /// </summary>
    /// <param name="option">the current option to be passed to the SkinData</param>
    private void UpdatePlayerOption(CustomizationOption option)
    {
        if (option is ColorOption colorOption)
        {
            skinData_SO.playerColor = colorOption.color;
            skinData_SO.playerColorIndex = currentOptionIndex;
        }
        if (option is MaterialOption matOption)
        {
            skinData_SO.playerMaterial = matOption.material;
            skinData_SO.playerMaterialIndex = currentOptionIndex;
        }
        
    }

}

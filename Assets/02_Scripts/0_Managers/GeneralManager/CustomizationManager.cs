using System;
using UnityEngine;

public class CustomizationManager : MonoBehaviour
{
    public static CustomizationManager Instance { get; private set; }

    public CustomizationData_SO customizationData_SO;
    public PlayerSkinData_SO skinData_SO;

    private CustomizationSlot currentSlot = null;
    private CustomizationOption currentOption = null;
    private int currentOptionIndex = 0;

    public event Action<CustomizationOption> OnOptionChanged;
    public event Action OnUpdatePlayerOption;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Sets the current option for the shop manager
    /// </summary>
    /// <param name="slot"></param>
    public void SetCurrentCustomizationSlot(CustomizationSlot slot)
    {
        currentSlot = slot;
        currentOption = slot.option;
        currentOptionIndex = slot.index;

        // This will call the PreviewOption() method on the PlayerCustomization.cs
        OnOptionChanged?.Invoke(currentOption);
    }

    public bool ValidatePurchase()
    {
        if (!CoinManager.Instance.CanAfford(currentOption.price.CoinType, currentOption.price.Amount))
        {
            // TODO: AudioPlay error sound
            return false;
        }
        else
        {
            CoinManager.Instance.ReduceCurrencyAmount(currentOption.price.CoinType, currentOption.price.Amount);
            UnlockOption(currentOption, currentOptionIndex);

            // TODO: Validation (V+S FX)
            AudioManager.Instance?.PlayValidate();

            UpdatePlayerOption(currentOption);

            return true;
        }
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
        if(option is SkinOption matOption)
        {
            customizationData_SO.skins[optionIndex].isLocked = false;
        }
        currentSlot.Unlock();
    }
    
    /// <summary>
    /// Updates the SkinData_SO with the given option
    /// </summary>
    /// <param name="option">the current option to be passed to the SkinData</param>
    private void UpdatePlayerOption(CustomizationOption option)
    {
        switch (option)
        {
            case ColorOption colorOpt:
                skinData_SO.playerColor = colorOpt.color;
                skinData_SO.playerColorIndex = currentOptionIndex;
                break;

            case SkinOption materialOpt:
                skinData_SO.playerSkin = materialOpt.skin;
                skinData_SO.playerSkinIndex = currentOptionIndex;
                skinData_SO.playerColor = materialOpt.skin.GetComponent<MeshRenderer>().material.color;
                skinData_SO.playerColorIndex = 0;
                break;

            default:
                return;
        }

        OnUpdatePlayerOption?.Invoke();
    }
}

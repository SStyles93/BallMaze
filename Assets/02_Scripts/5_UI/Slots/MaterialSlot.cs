using UnityEngine.EventSystems;


public class MaterialSlot : CustomizationSlot
{
    public SkinOption skinData;

    /// <summary>
    /// Initializes the Material Slot with it's options, index and a ref to the playerCustomization
    /// </summary>
    /// <param name="option">The MaterialOption passed</param>
    /// <param name="optionIndex">The index of the material</param>
    /// <param name="playerCustomization">Ref to the PlayerCustomization script</param>
    public void InitializeMaterialSlot(SkinOption option, int optionIndex, PlayerCustomization playerCustomization)
    {
        InitializeSlot(option, optionIndex, playerCustomization);

        skinData = option;
        slotImage.sprite = option.sprite;
    }
}

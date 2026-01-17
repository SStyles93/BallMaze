using UnityEngine;
using UnityEngine.UI;

public class SkinSlot : CustomizationSlot
{

    [SerializeField] private Sprite[] typeImages = new Sprite[2];
    [SerializeField] private Image typeImageRef;
    public SkinOption skinData;

    /// <summary>
    /// Initializes the Material Slot with it's options, index and a ref to the playerCustomization
    /// </summary>
    /// <param name="option">The MaterialOption passed</param>
    /// <param name="optionIndex">The index of the material</param>
    /// <param name="playerCustomization">Ref to the PlayerCustomization script</param>
    public void InitializeSkinSlot(SkinOption option, int optionIndex, PlayerCustomization playerCustomization)
    {
        InitializeSlot(option, optionIndex, playerCustomization);

        skinData = option;
        slotImage.sprite = option.sprite;
        // typesImage 0 => isColorable (paint brush) / 1 = Premium !Colorable
        typeImageRef.sprite = option.isColorable ? typeImages[0] : typeImages[1];
    }
}

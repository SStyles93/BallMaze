using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EquipmentManager : MonoBehaviour
{
    [Header("Project References")]
    [SerializeField] private PlayerSkinData_SO playerSkinData;
    // 0 = EQUIPPED / EQUIP = 1
    [SerializeField] private Sprite[] buttonSprites = new Sprite[2];

    [Header("Scene References")]
    [SerializeField] private GameObject actionButton;
    [SerializeField] private TMP_Text actionButtonText = null;
    private Image actionButtonImage;

    [Header("Runtime References")]
    [SerializeField] private CustomizationOption selectedOption = null;

    private void OnEnable()
    {
        CustomizationManager.Instance.OnOptionChanged += SetSelectedOption;
    }
    private void OnDisable()
    {
        CustomizationManager.Instance.OnOptionChanged -= SetSelectedOption;
    }

    private void Start()
    {
        actionButtonImage = actionButton.GetComponent<Image>();
    }

    /// <summary>
    /// Equips the selected option on the player<br/>
    /// Calls the Customization Manager Instance
    /// </summary>
    /// <remarks>Method called by the ActionButton</remarks>
    public void EquipSelectedOption()
    {
        CustomizationManager.Instance.UpdatePlayerSkinDataWithOption(selectedOption);
        SetActionButtonText(selectedOption);
    }

    public void ResetPlayerWithEquippedSkinOption()
    {
        CustomizationManager.Instance.SetSkinToCurrent();
    }

    private void SetSelectedOption(CustomizationSlot slot)
    {
        selectedOption = slot.option;
        if (!slot.option.isLocked)
        {
            SetActionButtonText(selectedOption);
            actionButton.gameObject.SetActive(true);
        }
        else
        {
            actionButton.gameObject.SetActive(false);
        }
    }

    private void SetActionButtonText(CustomizationOption option)
    {
        string textToDisplay = "";
        bool isEquipped = false;

        //EQUIP || EQUIPPED 
        if (option is SkinOption skinOption)
        {
            isEquipped = playerSkinData.skinOption == skinOption;
            
            textToDisplay = isEquipped ?
                "In Use" : "Use";
        }
        // APPLY || APPLIED 
        if (option is ColorOption colorOption)
        {
            isEquipped = playerSkinData.colorOption == colorOption;

            textToDisplay = playerSkinData.colorOption == colorOption ?
                "Applied" : "Apply";

        }
        actionButtonText.text = textToDisplay;

        // Dark (EQUIPED)/Clear (EQUIP) button colour 
        actionButtonImage.sprite = isEquipped ?
                buttonSprites[0] : buttonSprites[1];
    }
}

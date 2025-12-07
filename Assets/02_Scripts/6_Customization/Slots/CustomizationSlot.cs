using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomizationSlot : BaseUISlot
{
    private TMP_Text priceText;

    protected PlayerCustomization playerCustomization;

    public CustomizationOption option;

    public int index;

    public virtual void InitializeSlot(CustomizationOption option, int optionIndex, PlayerCustomization playerCustomization)
    {
        this.option = option;

        this.index = optionIndex;

        this.playerCustomization = playerCustomization;

        priceText ??= transform.GetComponentInChildren<TMP_Text>();
        priceText.enabled = false;
        priceText.text = $"{option.price.ToString()} $";

        if (option.isLocked)
        {
            this.isLocked = option.isLocked;
            lockImage.sprite = lockSprite;
            lockImage.color = lockColor;
        }
        else
        {
            lockImage.enabled = false;
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (isLocked)
        {
            priceText.enabled = true;

            //Reduce Visibility of the Lock & Slot
            ChangeImageVisibility(slotImage, .3f);
            ChangeImageVisibility(lockImage, .5f);
            
            return;
        }
        transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        //Debug.Log($"PointerEnter in {this.gameObject.name}");
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (isLocked)
        {
            priceText.enabled = false;

            ChangeImageVisibility(slotImage, 1f);
            ChangeImageVisibility(lockImage, 1f);

            return;
        }

        transform.localScale = new Vector3(1f, 1f, 1f);
        //slotBackground.color = slotColor;

        //Debug.Log($"PointerExit of {this.gameObject.name}");
    }

    /// <summary>
    /// Modifies the alpha value of a color
    /// </summary>
    /// <param name="slotImage">The image to change</param>
    /// <param name="alphaValue">the desired alpha value [0.0f - 1.0f]</param>
    private void ChangeImageVisibility(Image slotImage, float alphaValue)
    {
        Color tmpColor = slotImage.color;
        tmpColor.a = alphaValue;
        slotImage.color = tmpColor;
    }
}

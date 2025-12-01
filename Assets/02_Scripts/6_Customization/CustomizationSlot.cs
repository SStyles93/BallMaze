using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomizationSlot : BaseUISlot
{
    private TMP_Text priceText;

    protected PlayerCustomization playerCustomization;

    public virtual void InitializeSlot(CustomizationOption option, PlayerCustomization playerCustomization)
    {
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

            
            // TODO: NO WORKING
            //Reduce Visibility of the Lock
            Color tmpColor = lockImage.color;
            tmpColor.a = 50f;
            lockImage.color = tmpColor;
            
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

            // TODO: NO WORKING
            Color tmpColor = lockImage.color;
            tmpColor.a = 255f;
            lockImage.color = tmpColor;

            return;
        }

        transform.localScale = new Vector3(1f, 1f, 1f);
        //slotBackground.color = slotColor;

        //Debug.Log($"PointerExit of {this.gameObject.name}");
    }
}

using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MaterialSlot : BaseUISlot
{
    private MaterialOption skinData;

    private PlayerCustomization playerCustomization;

    public void InitializeMaterialSlot(MaterialOption option, PlayerCustomization playerCustomization)
    {
        skinData = option;
        slotImage.sprite = option.sprite;
        this.playerCustomization = playerCustomization;


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

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.dragging)
        {
            if (isLocked)
            {
                lockImage.color = lockColor;
                return;
            }

            //slotImage.color = Color.white;
            //Debug.Log($"Pointer was dragged on {this.gameObject.name}");
        }
        else // when the element is not dragged
        {
            if (isLocked) return;
            playerCustomization.AssignMaterial(skinData.material);
            //Debug.Log($"PointerUp on {this.gameObject.name}");
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (isLocked) return;
        transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        //Debug.Log($"PointerEnter in {this.gameObject.name}");
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (isLocked) return;

        transform.localScale = new Vector3(1f, 1f, 1f);
        //slotBackground.color = slotColor;

        //Debug.Log($"PointerExit of {this.gameObject.name}");
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (isLocked) return;
        //Debug.Log($"PointerDown on {this.gameObject.name}");
    }
}

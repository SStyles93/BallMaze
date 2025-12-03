using UnityEngine.EventSystems;


public class MaterialSlot : CustomizationSlot
{
    public MaterialOption skinData;

    public void InitializeMaterialSlot(MaterialOption option, int optionIndex, PlayerCustomization playerCustomization)
    {
        InitializeSlot(option, optionIndex, playerCustomization);

        skinData = option;
        slotImage.sprite = option.sprite;
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

            //Debug.Log($"Pointer was dragged on {this.gameObject.name}");
        }
        else // when the element is not dragged
        {
            if (isLocked)
            {
                ShopManager.Instance.SetCurrentCustomizationSlot(this);
                return;
            }
            playerCustomization.AssignMaterial(skinData.material);
            playerCustomization.AssignMaterialIndex(index);
            //Debug.Log($"PointerUp on {this.gameObject.name}");
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (isLocked)return;
        //Debug.Log($"PointerDown on {this.gameObject.name}");
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomizationSlot : BaseUISlot
{
    protected PlayerCustomization playerCustomization;
    public CustomizationOption option;
    public bool isLockedByLevel = false;
    public int index;

    [SerializeField] private TMP_Text levelText;


    public virtual void InitializeSlot(CustomizationOption option, int optionIndex, PlayerCustomization playerCustomization)
    {
        this.option = option;

        this.index = optionIndex;

        this.playerCustomization = playerCustomization;

        isLockedByLevel = option.levelToUnlock > LevelManager.Instance.GetHighestFinishedLevelIndex();
        // Locked by level
        if (isLockedByLevel)
        {
            levelText.text = option.levelToUnlock.ToString();
            levelText.enabled = true;
        }
        else
            levelText.enabled = false;

        // Locked by purchase
        if (option.isLocked)
        {
            this.isLocked = option.isLocked;
            lockImage.sprite = lockSprite;
            lockImage.color = lockColor;
        }
        else
            lockImage.enabled = false;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        if (isLocked)
        {
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
        base.OnPointerExit(eventData);

        if (isLocked)
        {
            ChangeImageVisibility(slotImage, 1f);
            ChangeImageVisibility(lockImage, 1f);

            return;
        }

        transform.localScale = new Vector3(1f, 1f, 1f);
        //slotBackground.color = slotColor;

        //Debug.Log($"PointerExit of {this.gameObject.name}");
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        // If click released when pointer is dragged
        if (eventData.dragging)
        {
            if (isLocked)
            {
                lockImage.color = lockColor;
                return;
            }

            //Debug.Log($"Pointer was dragged on {this.gameObject.name}");
        }
        // If the click released when over the slot
        else if (isMouseOver)
        {
            //Reset rotation of the viewer
            playerCustomization.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));

            CustomizationManager.Instance.SetCurrentCustomizationSlot(this);

            //Debug.Log($"PointerUp on {this.gameObject.name}");
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BaseUISlot : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    // Slot parameters
    [SerializeField] protected Image slotImage;
    [SerializeField] protected Image lockImage;

    // Locked Parameters
    [SerializeField] protected Sprite lockSprite;
    [SerializeField] protected Color lockColor = Color.orange;

    protected bool isLocked = false;
    protected bool isMouseOver = false;

    /// <summary>
    /// Method called when pointer is over the object and released
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.dragging)
        {
            if (isLocked)
            {
                lockImage.color = lockColor;
                return;
            }

            lockImage.color = Color.white;
            //Debug.Log($"Pointer was dragged on {this.gameObject.name}");
        }
        else
        {
            if (isLocked) return;
            if(isMouseOver) AudioManager.Instance?.PlayClickSound();
            //Debug.Log($"PointerUp on {this.gameObject.name}");
        }
    }

    /// <summary>
    /// Method called when the pointer is click (down) on slot
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (isLocked) return;
        //Debug.Log($"PointerDown on {this.gameObject.name}");
    }

    /// <summary>
    /// Called when the pointer hovers over the slot
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
        if (isLocked) return;
        transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        //Debug.Log($"PointerEnter in {this.gameObject.name}");
    }

    /// <summary>
    /// Called when the pointer exits the slot (out of hover)
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
        if (isLocked) return;
        transform.localScale = new Vector3(1f, 1f, 1f);
        //Debug.Log($"PointerExit of {this.gameObject.name}");
    }

    /// <summary>
    /// Removes the lock image and Unlocks the slot
    /// </summary>
    public void Unlock()
    {
        isLocked = false;
        lockImage.enabled = false;
    }
}

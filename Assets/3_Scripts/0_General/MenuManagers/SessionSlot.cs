using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SessionSlot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image slotBackground;
    [SerializeField] private Image slotImage;
    [SerializeField] private TMP_Text slotText;

    private string sessionID;

    public void InitializeSessionSlot()
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //Load game
        //SessionManager.Instance.
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        slotBackground.color = Color.gray;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        slotBackground.color = Color.black;
    }
}

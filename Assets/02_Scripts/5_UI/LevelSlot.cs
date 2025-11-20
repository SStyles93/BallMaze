using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelSlot : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image slotBackground;
    [SerializeField] private Image slotImage;
    [SerializeField] private TMP_Text slotText;

    [SerializeField] private Sprite lockedImage;

    [SerializeField] private Color lockedColor = Color.orange;

    private bool isLocked = false;
    private int slotIndex;

    public void InitializeLevelSlot(int level, bool isLocked = false)
    {
        slotIndex = level;
        this.isLocked = isLocked;

        if (this.isLocked)
        {
            slotImage.sprite = lockedImage;
            slotText.enabled = false;
        }
        else
        {
            slotImage.enabled = false;
            slotText.text = level.ToString();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isLocked) return;
        slotImage.color = new Color(1, 1, 1, 0.5f);
        //Debug.Log($"PointerDown on {this.gameObject.name}");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.dragging)
        {
            if (isLocked)
            {
                slotImage.color = lockedColor;
                return;
            }

            slotImage.color = Color.white;
            //Debug.Log($"Pointer was dragged on {this.gameObject.name}");
        }
        else
        {
            if (isLocked) return;

            GamesMenuManager.Instance.SaveScrollbarValues();

            LevelManager.Instance.InitializeLevel(slotIndex);

            SceneController.Instance
                .NewTransition()
                .Load(SceneDatabase.Slots.Content, SceneDatabase.Scenes.Game)
                .Unload(SceneDatabase.Scenes.GamesMenu)
                .WithOverlay()
                .Perform();
            //Debug.Log($"PointerUp on {this.gameObject.name}");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        slotBackground.color = Color.gray;

        //Debug.Log($"PointerEnter in {this.gameObject.name}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isLocked)
        {
            slotImage.color = lockedColor;
        }
            
        slotBackground.color = Color.white;

        //Debug.Log($"PointerExit of {this.gameObject.name}");
    }
}

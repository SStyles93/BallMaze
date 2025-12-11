using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelSlot : BaseUISlot
{
    [SerializeField] private TMP_Text slotText;
    [SerializeField] private Image[] starImages;

    private int slotIndex;

    public void InitializeLevelSlot(int levelIndex, bool isLocked = false)
    {
        slotIndex = levelIndex;
        this.isLocked = isLocked;

        if (this.isLocked)
        {
            lockImage.sprite = lockSprite;
            slotText.enabled = false;
        }
        else
        {
            lockImage.enabled = false;
            slotText.text = levelIndex.ToString();

            foreach (var image in starImages)
                image.enabled = true;

            int grade = LevelManager.Instance.GetGradeForLevelAtIndex(levelIndex);
            for (int i = 0; i < grade; i++)
            {
                starImages[i].color = Color.white;
            }
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if (eventData.dragging)
        {
            if (isLocked)
            {
                lockImage.color = lockColor;
                return;
            }

            lockImage.color = Color.white;
        }
        else if(isMouseOver)
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

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (isLocked) return;
        lockImage.color = new Color(1, 1, 1, 0.5f);
        //Debug.Log($"PointerDown on {this.gameObject.name}");
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
        slotImage.color = Color.gray;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;

        if (isLocked)
        {
            lockImage.color = lockColor;
        }

        slotImage.color = Color.white;
    }
}

using TMPro;
using UnityEngine;

public class StarCountPannel : MonoBehaviour
{
    [SerializeField] private TMP_Text starCountText;

    private void OnEnable()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnStarCountChanged += UpdateStarCountText;
    }

    private void OnDisable()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnStarCountChanged -= UpdateStarCountText;
    }

    public void UpdateStarCountText(int starCount)
    {
        starCountText.text = $"{starCount}/3";
    }
}

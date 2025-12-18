using UnityEngine;
using UnityEngine.UI;

public class UITabButton : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Color selectedColor = PxP.BallMaze.Color.BlueLight;
    [SerializeField] Color unselectedColor = PxP.BallMaze.Color.BlueDark;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void SetToSelectedColour()
    {
        image.color = selectedColor;
    }

    public void SetToUnselectedColour()
    {
        image.color = unselectedColor;
    }
}

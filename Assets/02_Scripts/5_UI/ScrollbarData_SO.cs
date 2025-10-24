using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ScrollbarValues", menuName = "UI/ScrollbarValue")]
public class ScrollbarData_SO : ScriptableObject
{
    public float scrollbarValue;
    public float scrollbarSize;

    public void SetScrollbarValues(Scrollbar scrollBar)
    {
        scrollbarValue = scrollBar.value;
        scrollbarSize = scrollBar.size;
    }
}

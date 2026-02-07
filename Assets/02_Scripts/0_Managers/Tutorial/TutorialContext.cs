using UnityEngine;

public class TutorialContext : MonoBehaviour
{
    [System.Serializable]
    public struct UIAnchor
    {
        public string id;
        public RectTransform rect;
    }

    public UIAnchor[] anchors;

    //Check if tutorial was played TutorialManager & SavingSystem

    private void Awake()
    {
        TutorialManager.Instance.RegisterContext(this);
    }

    public RectTransform Get(string id)
    {
        foreach (var a in anchors)
            if (a.id == id)
                return a.rect;

        Debug.LogWarning($"Tutorial anchor '{id}' not found");
        return null;
    }
}

using UnityEngine;

public class CustomizationTutorialManager : MonoBehaviour
{
    private bool throwTutorial = false;

    private void Awake()
    {
        if (TutorialManager.Instance == null) return;
        throwTutorial = !TutorialManager.Instance.IsTutorialShopComplete;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (throwTutorial)
            TutorialManager.Instance.StartTutorial("ShopTutorial");
    }
}

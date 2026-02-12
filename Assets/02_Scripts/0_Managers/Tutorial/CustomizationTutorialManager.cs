using UnityEngine;

public class CustomizationTutorialManager : MonoBehaviour
{
    [SerializeField] CustomizationData_SO customizationData_SO;
    private bool throwTutorial = false;

    private void Awake()
    {
        if (TutorialManager.Instance == null) return;
        throwTutorial = !TutorialManager.Instance.IsTutorialShopComplete;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (throwTutorial 
            && customizationData_SO.skins[2].isLocked
            && customizationData_SO.colors[2].isLocked)
            TutorialManager.Instance.StartTutorial("ShopTutorial");
    }
}

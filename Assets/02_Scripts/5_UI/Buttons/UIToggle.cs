using UnityEngine;
using UnityEngine.UI;

public class UIToggle : UIButton
{
    [Header("Object Reference")]
    [SerializeField] protected GameObject crossImage;

    [SerializeField] protected bool isOn = true;

    protected override void Start()
    {
        base.Start();
        button.onClick.AddListener(Toggle);

        //Debug.Log($"{this.gameObject.name} Started");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        button.onClick.RemoveListener(Toggle);

        //Debug.Log($"{this.gameObject.name} Started");
    }

    protected virtual void InitializeToggle(bool isOn = true)
    {
        this.isOn = isOn;
        // Cross image apears when the button is not ON
        crossImage.SetActive(!isOn);

        //Debug.Log($"{this.gameObject.name} initialized");
    }

    protected virtual void Toggle()
    {
        isOn = !isOn;
        crossImage.SetActive(!isOn);

        //Debug.Log($"{this.gameObject.name} Toggle()");
    }
}

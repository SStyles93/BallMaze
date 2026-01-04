using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    protected Button button;

    private void Awake()
    {
        if(button == null) button = GetComponent<Button>();

        //Debug.Log($"{this.gameObject.name} Initialized");
    }

    protected virtual void Start()
    {
        button.onClick.AddListener(PlayClickSound);
        
        //Debug.Log($"{this.gameObject.name} Started");
    }

    protected virtual void OnDestroy()
    {
        button.onClick.RemoveListener(PlayClickSound);
    }

    protected virtual void PlayClickSound()
    {
        AudioManager.Instance?.PlayClickSound();
    }
}

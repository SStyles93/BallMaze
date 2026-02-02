using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    [SerializeField] protected Button button;

    protected virtual void Awake()
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

    protected virtual void OnEnable()
    {
        
    }

    protected virtual void OnDisable()
    {
        
    }
}

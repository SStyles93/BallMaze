using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        if(button == null) button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(PlayClickSound);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(PlayClickSound);
    }

    private void PlayClickSound()
    {
        AudioManager.Instance?.PlayClickSound();
    }
}

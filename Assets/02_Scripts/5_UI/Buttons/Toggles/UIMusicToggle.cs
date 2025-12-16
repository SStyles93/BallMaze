using UnityEngine;

public class UIMusicToggle : UIToggle
{
    protected override void Start()
    {
        base.Start();
        InitializeToggle(AudioManager.Instance.IsMusicEnabled);
    }

    protected sealed override void Toggle()
    {
        base.Toggle();
        AudioManager.Instance?.SetMusicState(isOn);
    }
}

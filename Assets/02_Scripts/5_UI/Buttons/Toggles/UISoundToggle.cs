using UnityEngine;

public class UISoundToggle : UIToggle
{
    protected override void Start()
    {
        base.Start();
        InitializeToggle(AudioManager.Instance.IsAudioEnabled);
    }

    protected sealed override void Toggle()
    {
        base.Toggle();
        AudioManager.Instance?.SetGeneralAudioState(isOn);
    }
}

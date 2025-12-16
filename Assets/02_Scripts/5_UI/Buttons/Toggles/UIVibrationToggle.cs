using UnityEngine;

public class UIVibrationToggle : UIToggle
{
    protected override void Start()
    {
        base.Start();
        InitializeToggle(VibrationManager.Instance.IsVibrationActive);
    }

    protected sealed override void Toggle()
    {
        base.Toggle();
        VibrationManager.Instance?.SetVibrationManagerState(isOn);
    }
}

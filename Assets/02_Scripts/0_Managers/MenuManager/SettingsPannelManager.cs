using UnityEngine;

public class SettingsPannelManager : MonoBehaviour
{
    public void CloseSettingsPannel()
    {
        SavingManager.Instance?.SaveSettings();

        SceneController.Instance?.NewTransition()
          .Unload(SceneDatabase.Scenes.SettingsPannel)
          .Perform();
    }

    public void OpenCreditsMenu()
    {
        SavingManager.Instance?.SaveSettings();

        SceneController.Instance.NewTransition()
          .Unload(SceneDatabase.Scenes.CreditsPannel)
          .Perform();
    }
}

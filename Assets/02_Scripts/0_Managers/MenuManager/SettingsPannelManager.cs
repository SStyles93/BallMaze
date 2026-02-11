using UnityEngine;

public class SettingsPannelManager : MonoBehaviour
{
    public void CloseSettingsPannel()
    {
        SavingManager.Instance?.SaveSettings();

        SceneController.Instance?.NewTransition()
            .Unload(SceneDatabase.Scenes.SettingsPannel)
            .SetActive(SceneDatabase.Scenes.GamesMenu)
            .Perform();
    }

    public void OpenCreditsMenu()
    {
        SavingManager.Instance?.SaveSettings();

        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.CreditsPannel)
            .Perform();
    }

    public void OnForceSaveData()
    {
        CloudSaveManager.Instance?.ForceCloudSave();
    }

    public void OnForceDeleteData()
    {
        CloudSaveManager.Instance?.ForceDeleteCloudData();
    }
}

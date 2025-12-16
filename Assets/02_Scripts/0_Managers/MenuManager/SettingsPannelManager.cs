using UnityEngine;

public class SettingsPannelManager : MonoBehaviour
{
    public void CloseSettingsPannel()
    {
        SceneController.Instance.NewTransition()
          .Unload(SceneDatabase.Scenes.SettingsPannel)
          .Perform();
    }
}

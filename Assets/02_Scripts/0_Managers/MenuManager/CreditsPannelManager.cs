using UnityEngine;

public class CreditsPannelManager : MonoBehaviour
{
    public void CloseCreditsPannel()
    {
        SceneController.Instance?.NewTransition()
            .Unload(SceneDatabase.Scenes.CreditsPannel)
            .SetActive(SceneDatabase.Scenes.SettingsPannel)
            .Perform();
    }
}

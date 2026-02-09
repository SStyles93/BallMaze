using UnityEngine;

public class TutorialUfoModificator : TutorialReceiver
{
    [SerializeField] private PowerUpManager powerUpManager;

    public override void OnTutorialSignal(TutorialSignal signal)
    {
        switch (signal.id)
        {
            case "tutorial/ufo/infinitetime":
                powerUpManager.SetInfinitTime(true);
                break;

            case "tutorial/ufo/stop":
                powerUpManager.SetInfinitTime(false);
                break;


        }
    }
}
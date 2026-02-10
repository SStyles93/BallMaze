using UnityEngine;

public class TutorialPlayerReceiver : TutorialReceiver
{
    [SerializeField] PlayerMovement playerMovement;

    private void Start()
    {
        if (playerMovement == null) 
            playerMovement = GameObject
                .FindGameObjectWithTag("Player")
                .GetComponent<PlayerMovement>();
    }

    public override void OnTutorialSignal(TutorialSignal signal)
    {
        playerMovement.ForceJump();
    }
}

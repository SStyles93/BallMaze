using UnityEngine;

public class GameTutorialManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TutorialManager.Instance.StartTutorial(0);
    }
}

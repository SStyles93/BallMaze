using System;
using System.Collections.Generic;
using UnityEngine;

public class GameTutorialManager : MonoBehaviour
{
    public List<int> triggers = new List<int>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TutorialManager.Instance.StartTutorial(0);
    }

    // Use a dictionary to store callbacks per trigger index

    // Register a callback for a trigger index
    public void RegisterTrigger(int index)
    {
        if (!triggers.Contains(index))
            triggers.Add(index);
    }

    // Called by the trigger when something enters it
    public void TriggerActivated(int index)
    {
        if (triggers.Contains(index))
        {
            TutorialManager.Instance.StartTutorial(index);
            triggers.RemoveAt(index);
        }
    }
}

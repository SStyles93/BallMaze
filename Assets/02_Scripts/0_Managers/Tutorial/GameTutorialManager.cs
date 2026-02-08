using System;
using System.Collections.Generic;
using UnityEngine;

public class GameTutorialManager : MonoBehaviour
{
    public Dictionary<int, Action> triggeredCallbacks = new Dictionary<int, Action>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TutorialManager.Instance.StartTutorial(0);
    }

    // Use a dictionary to store callbacks per trigger index

    // Register a callback for a trigger index
    public void RegisterTrigger(int index, Action callback)
    {
        if (!triggeredCallbacks.ContainsKey(index))
        {
            triggeredCallbacks.Add(index, callback);
        }
    }

    // Called by the trigger when something enters it
    public void TriggerActivated(int index)
    {
        if (triggeredCallbacks.TryGetValue(index, out var callback))
        {
            callback?.Invoke();
            triggeredCallbacks.Remove(index);
        }
    }
}

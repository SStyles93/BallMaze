using System.Collections.Generic;
using UnityEngine;

public struct TutorialSignal
{
    public string id;
    public object payload;

    public TutorialSignal(string id, object payload = null)
    {
        this.id = id;
        this.payload = payload;
    }
}
public interface ITutorialReceiver
{
    void OnTutorialSignal(TutorialSignal signal);
}

public abstract class TutorialReceiver : MonoBehaviour, ITutorialReceiver
{
    [SerializeField]
    protected List<string> listenToSignalIds;

    protected virtual void OnEnable()
    {
        TutorialBus.OnSignal += HandleSignal;
    }

    protected virtual void OnDisable()
    {
        TutorialBus.OnSignal -= HandleSignal;
    }

    private void HandleSignal(TutorialSignal signal)
    {
        foreach(string signalId in listenToSignalIds)
        {
            if (signal.id == signalId)
                OnTutorialSignal(signal);
        }
    }

    /// <summary>
    /// Receives the tutorial signals that are equal to ones entered in the ListenToSignalIds<br/>
    /// For single signals it is not necessary to re-check the signal ID
    /// </summary>
    /// <param name="signal">Signal ID</param>
    public abstract void OnTutorialSignal(TutorialSignal signal);
}

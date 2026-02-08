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

public interface ITutorialSceneObject
{
    void OnTutorialSignal(TutorialSignal signal);
}

public abstract class TutorialSceneObject
    : MonoBehaviour, ITutorialSceneObject
{
    [SerializeField]
    protected string listenToSignalId;

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
        if (signal.id == listenToSignalId)
            OnTutorialSignal(signal);
    }

    public abstract void OnTutorialSignal(TutorialSignal signal);
}
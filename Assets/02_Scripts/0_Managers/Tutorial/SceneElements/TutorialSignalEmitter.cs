using UnityEngine;
public class TutorialSignalEmitter : MonoBehaviour
{
    public void Emit(string signalId)
    {
        TutorialBus.Raise(signalId);
    }
}
public static class TutorialBus
{
    public static event System.Action<TutorialSignal> OnSignal;

    public static void Raise(string id, object payload = null)
    {
        OnSignal?.Invoke(new TutorialSignal(id, payload));
    }
}

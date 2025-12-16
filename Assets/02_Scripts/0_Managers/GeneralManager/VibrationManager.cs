using System.Collections;
using UnityEngine;

public class VibrationManager : MonoBehaviour
{
    public static VibrationManager Instance { get; private set; }
    public bool IsVibrationActive => isVibrationActive;

    private Coroutine currentVibrationRoutine = null;

    private bool isVibrationActive = true;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Vibration.Init();
    }

    /// <summary>
    /// 400ms Vibration
    /// </summary>
    public void Classic()
    {
        if (!isVibrationActive) return;

        Vibration.Vibrate();
    }

    /// <summary>
    /// Calls short vibrations
    /// </summary>
    /// <param name="numberOfPops">number of vibrations</param>
    /// <param name="period">time span over which the vibrations are called</param>
    public void MultiPop(int numberOfPops, float period = 0.6f)
    {
        if (!isVibrationActive) return;

        if (currentVibrationRoutine != null) StopCoroutine(currentVibrationRoutine);

        currentVibrationRoutine = StartCoroutine(MultiPopRoutine(numberOfPops, period));
    }

    /// <summary>
    /// Unique short vibration
    /// </summary>
    public void Pop()
    {
        if (!isVibrationActive) return;

        Vibration.VibratePop();
    }

    public void SetVibrationManagerState(bool isActive)
    {
        this.isVibrationActive = isActive;
    }

    /// <summary>
    /// Multiple short vibrations (Routine)
    /// </summary>
    /// <param name="numberOfPops">number of vibrations</param>
    /// <param name="period">time span over which the vibrations are called</param>
    /// <returns>Wait for seconds (time span divided by number of pops</returns>
    private IEnumerator MultiPopRoutine(int numberOfPops, float period)
    {
        for (int i = 0; i < numberOfPops; i++)
        {
            Vibration.VibratePop();
            yield return new WaitForSeconds(period/numberOfPops);
        }
    }

}

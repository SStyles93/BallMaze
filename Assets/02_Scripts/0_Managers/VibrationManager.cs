using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class VibrationManager : MonoBehaviour
{
    public static VibrationManager Instance { get; private set; }

    private Coroutine currentVibrationRoutine = null;


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
        Vibration.Vibrate();
    }

    /// <summary>
    /// Unique short vibration
    /// </summary>
    public void Pop()
    {
        Vibration.VibratePop();
    }

    /// <summary>
    /// Calls short vibrations
    /// </summary>
    /// <param name="numberOfPops">number of vibrations</param>
    /// <param name="period">time span over which the vibrations are called</param>
    public void MultiPop(int numberOfPops, float period = 0.6f)
    {
        if(currentVibrationRoutine != null) StopCoroutine(currentVibrationRoutine);

        currentVibrationRoutine = StartCoroutine(MultiPopRoutine(numberOfPops,period));
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

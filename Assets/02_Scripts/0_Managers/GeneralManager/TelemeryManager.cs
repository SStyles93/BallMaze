//using UnityEngine;
//using System.Collections.Generic;

//public class TelemetryManager : MonoBehaviour
//{
//    public static TelemetryManager Instance { get; private set; }

//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(this.gameObject);
//            return;
//        }
//        Instance = this;
//        DontDestroyOnLoad(this.gameObject);
//    }

//    // Example: store events locally first
//    private List<LevelTelemetryEvent> _eventBuffer = new List<LevelTelemetryEvent>();

//    public void LogLevelEvent(LevelTelemetryEvent e)
//    {
//        _eventBuffer.Add(e);

//        // Debug log for development
//        Debug.Log($"[Telemetry] Level {e.levelIndex} - {e.archetypeName} - {e.result}");

//        // TODO: Send to analytics backend here (e.g., Firebase, Unity Analytics, custom server)
//        // SendEventToServer(e);
//    }

//    public string GetDominantModifier(LevelArchetypeData_SO archetype)
//    {
//        if (archetype == null || archetype.modifiers == null)
//            return "None";

//        float maxWeight = -1f;
//        ModifierType dominant = ModifierType.Empty;

//        foreach (var mod in archetype.modifiers)
//        {
//            if (mod.weight > maxWeight)
//            {
//                maxWeight = mod.weight;
//                dominant = mod.type;
//            }
//        }

//        return dominant.ToString();
//    }
//}

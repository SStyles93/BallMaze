using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "LevelCycleProgression",
    menuName = "Level Progression/Cycle Progression")]
public class LevelCycleProgression_SO : ScriptableObject
{
    [Tooltip("Recovery archetype used when recovery is triggered")]
    public LevelArchetypeData_SO recoveryArchetype;

    [Tooltip("Ordered list of cycles. Index = cycleIndex.")]
    public List<LevelCycleDefinition> cycles = new List<LevelCycleDefinition>();
}

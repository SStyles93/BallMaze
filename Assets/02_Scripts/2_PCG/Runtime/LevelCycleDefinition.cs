using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelCycleDefinition
{
    [Tooltip("Optional name for clarity in the inspector (e.g. 'Precision Intro')")]
    public string cycleName;

    [Tooltip("Archetypes allowed during this cycle")]
    public List<LevelArchetypeData_SO> allowedArchetypes;
}
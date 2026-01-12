using UnityEngine;

[CreateAssetMenu(menuName = "Level Progression/Archetype Database")]
public class LevelArchetypeDatabase_SO : ScriptableObject
{
    [Header("Core Archetypes")]
    public LevelArchetypeData_SO recovery;

    [Header("Single Modifier")]
    public LevelArchetypeData_SO precision;
    public LevelArchetypeData_SO slippery;
    public LevelArchetypeData_SO timing;

    [Header("Dual Modifier")]
    public LevelArchetypeData_SO precisionSlippery;
    public LevelArchetypeData_SO precisionTiming;
    public LevelArchetypeData_SO slipperyTiming;
}

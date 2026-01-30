using UnityEngine;
using System;

public enum ModifierType
{
    Empty,
    Ice,
    Moving,
    Piques,
    DoorDown,
    DoorUp
    // **************************
    // ADD ANY MODIFIER TYPE HER
    // **************************
}

[CreateAssetMenu(menuName = "Level Progression/Level Archetype")]
public class LevelArchetypeData_SO : ScriptableObject
{
    [Serializable]
    public struct ModifierWeight
    {
        public ModifierType type;
        [Range(0f, 1f)]
        public float weight; // 0 = inactive, 1 = dominant
    }

    [Header("Design")]
    public string archetypeName;
    public ModifierWeight[] modifiers;

    [Header("Constraints")]
    [Tooltip("Max number of active modifiers (enforced at runtime)")]
    public int maxActiveModifiers = 2;
}

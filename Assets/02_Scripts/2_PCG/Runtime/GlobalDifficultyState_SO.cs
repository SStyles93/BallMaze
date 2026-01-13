using UnityEngine;


[CreateAssetMenu(fileName = "Global Difficulty State", menuName = "Level Progression/Global Difficulty State")]
public class GlobalDifficultyState_SO : ScriptableObject
{
    public float difficultyDebt;   // 0 → no easing, 1 → max easing
    public int remainingLevels;    // how long it lasts
}
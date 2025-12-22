using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Procedural Generation/Level Database")]
public class LevelDatabase_SO : ScriptableObject
{
    public List<LevelData_SO> levels = new List<LevelData_SO>();
}

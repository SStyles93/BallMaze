using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Procedural Generation/Level Database")]
public class LevelDatabase_SO : ScriptableObject
{
    public List<LevelData_SO> levels = new List<LevelData_SO>();

    public void SaveLevelData(LevelData_SO data)
    {
        int existingIndex = levels.FindIndex(l => l.index == data.index);

        if (existingIndex >= 0)
            levels[existingIndex] = data; // override
        else
            levels.Add(data); // new entry

        levels = levels.OrderBy(level => level.index).ToList();

        Debug.Log($"LevelData_SO saved at index [{data.index}]");
    }

    /// <summary>
    /// Returns the LevelData with the given index 
    /// </summary>
    /// <param name="index">index of the level</param>
    /// <returns>LevelData_SO</returns>
    public LevelData_SO GetLevelDataAtIndex(int index)
    {
        LevelData_SO foundItem = levels.Find(level => level.index == index);

        if (foundItem == null)
        {
            //Debug.Log($"No LevelData found with index of [{index}]");
            return null;
        }
        return foundItem;
    }

    public int LevelCount()
    {
        return levels.Count;
    }
}

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class LevelParameters
{
    [Header("Seed")]
    public int Seed = -1;
    [Header("Path Settings")]
    public int Spacing = 5;
    public float PathDensity = 50; // 0-100
    public float PathTwistiness = 50; // 0 = straight, 50 = neutral, 100 = twisty
    public int PathWidth = 2;
    public bool AllowBranching = false;

    [Header("Map Size")]
    public int MaxX = 20;
    public int MaxZ = 20;

    [Header("Level Settings")]
    public float timeToComplete = 10;

    [Space(10)]
    public bool GeneratedAutomatically = false;
}

[CreateAssetMenu(fileName = "LevelsPcgData", menuName = "Procedural Generation/PCG Data")]
public class PcgData_SO : ScriptableObject
{
    public List<LevelParameters> levelParameters = new List<LevelParameters>();

    public void DeleteAutoGenData()
    {
        for (int i = 0; i < levelParameters.Count; i++)
        {
            if (levelParameters[i].GeneratedAutomatically)
            {
                levelParameters.RemoveAt(i);
                i--;
            }
        }
    }

    /// <summary>
    /// Adds a generation parameters field to the list
    /// </summary>
    /// <param name="gen">The current GenerationParameter_SO used</param>
    public void AddDataParameter(GenerationParamameters_SO gen, int indexOfDataToWrite = -1)
    {
        LevelParameters levelParameter = new LevelParameters()
        {
            Seed = gen.currentSeed,
            Spacing = gen.Spacing,
            PathDensity = gen.PathDensity,
            PathTwistiness = gen.PathTwistiness,
            PathWidth = gen.PathWidth,
            AllowBranching = gen.AllowBranching,
            MaxX = gen.MaxX,
            MaxZ = gen.MaxZ,
            timeToComplete = 10.0f
        };

        if (indexOfDataToWrite == -1)
            levelParameters.Add(levelParameter);
        else
            if (levelParameters[indexOfDataToWrite] != null)
        {
            levelParameters[indexOfDataToWrite] = levelParameter;
        }
    }

    public void GetDataFromSO(GenerationParamameters_SO gen, int indexOfDataToGet)
    {
        if (indexOfDataToGet == -1) return;
        if (levelParameters.Count < indexOfDataToGet)
        {
            Debug.Log($"No LevelParameter at index {indexOfDataToGet}");
            return;
        }

        gen.currentSeed = levelParameters[indexOfDataToGet].Seed;
        gen.Spacing = levelParameters[indexOfDataToGet].Spacing;
        gen.PathDensity = levelParameters[indexOfDataToGet].PathDensity;
        gen.PathTwistiness = levelParameters[indexOfDataToGet].PathTwistiness;
        gen.PathWidth = levelParameters[indexOfDataToGet].PathWidth;
        gen.AllowBranching = levelParameters[indexOfDataToGet].AllowBranching;
        gen.MaxX = levelParameters[indexOfDataToGet].MaxX;
        gen.MaxZ = levelParameters[indexOfDataToGet].MaxZ;
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(PcgData_SO))]
    public class PcgData_SO_CUstomInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            PcgData_SO _target = (PcgData_SO)target;

            if (GUILayout.Button("Delete Auto Generated Data"))
            {
                _target.DeleteAutoGenData();
            }
        }
    }
#endif
}

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class LevelParameters
{
    public int Seed = -1;
    public int Spacing = 5;
    public float PathDensity = 50; // 0-100
    public float PathTwistiness = 50; // 0 = straight, 50 = neutral, 100 = twisty
    public int PathWidth = 2;
    public bool AllowBranching = false;
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

using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "GenerationParameters", menuName = "Procedural Generation/Generation Parameters")]
public class GenerationParamameters_SO : ScriptableObject
{
    public int Seed = -1;

    [Header("Map Points")]
    public Vector2Int StartPos = new Vector2Int(20, 0);
    public bool PlaceEndManually = false;
    [ConditionalField("PlaceEndManually")]
    public Vector2Int EndPos;
    public int MinEndDistance = 0; // Only used when PlaceEndManually is false

    [Header("Path Style")]
    public int Spacing = 5; // 0 = no space between paths, 10 = 10 tile space between paths
    public float PathDensity = 50; // 0-100
    public float PathTwistiness = 50; // 0 = straight, 50 = neutral, 100 = twisty
    public int PathWidth = 2; // Width of the path 5 = large (easy), 1 = small path (hard) 
    public bool AllowBranching = false;

    [Header("Map Size")]
    public int MaxX = 40;
    public int MaxZ = 40;
    public int MapDepth = 50;

    public void ResetParameters()
    {
        Seed = -1;
        StartPos = new Vector2Int(20, 0);
        PlaceEndManually = false;

        MinEndDistance = 0; // Only used when PlaceEndManually is false

        Spacing = 5;
        PathDensity = 50; // 0-100
        PathTwistiness = 50; // 0 = straight, 50 = neutral, 100 = twisty
        PathWidth = 2;
        AllowBranching = false;

        MaxX = 40;
        MaxZ = 40;
        MapDepth = 50;
    }
}
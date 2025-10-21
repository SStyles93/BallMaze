using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "GenerationParameters", menuName = "ProceduralGeneration/GenerationParameters")]
public class GenerationParams : ScriptableObject
{
    public int Seed = -1;

    [Header("Map Points")]
    public Vector2Int StartPos = new Vector2Int(10, 0);
    public bool PlaceEndManually = false;
    [ConditionalField("PlaceEndManually")]
    public Vector2Int EndPos;
    public int MinEndDistance = 0; // Only used when PlaceEndManually is false
    
    [Header("Path Style")]
    public int Spacing = 3;
    public float PathDensity = 50; // 0-100
    public float PathTwistiness = 50; // 0 = straight, 50 = neutral, 100 = twisty
    public int PathWidth = 2;
    public bool AllowBranching = false;

    [Header("Map Size")]
    public int MaxX = 20;
    public int MaxZ = 20;
}
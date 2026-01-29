using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Generation Parameters",
    menuName = "Procedural Generation/Parameters"
)]
public class GeneratorParameters_SO : ScriptableObject
{
    [Header("Grid Settings")]
    public int gridWidth = 10;
    public int gridHeight = 10;

    [Header("End Settings")]
    public bool randomEnd = true;
    public Vector2Int fixedEnd = new(9, 9);
    public int endMaxHeightPercent = 20;

    [Header("Seed Settings")]
    [Tooltip("-1 = random seed")]
    public int inputSeed = -1;

    [Header("Path Settings")]
    public TileDatabase_SO tileDatabase;
    public Dictionary<GroundType, float> tileRatios;

    [Header("Stars & Currencies")]
    [Range(0, 20)]
    public int starCount = 3;
    [Range(0, 99999)]
    public int coinsToEarn = 30;

    [Range(1, 10)]
    [Tooltip("Minimum distance between stars")]
    public int minStarDistance = 2;

    [Tooltip("If true, stars will also have a path connecting to the end position")]
    public bool starsConnectToEnd = false;
}

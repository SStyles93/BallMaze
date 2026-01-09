using System;
using Unity.VisualScripting;
using UnityEngine;

public class PhysicalMazeGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject startPrefab;
    public GameObject endPrefab;
    public GameObject starPrefab;
    public GameObject icePrefab;

    [Header("Layout")]
    public float cellSize = 1f;
    public bool clearPrevious = true;

    [Header("Visuals")]
    [SerializeField] private EnvironmentColors_SO environmentColors_SO;
    [SerializeField] public Material material = null;
    [SerializeField] public Material emissiveBandMaterial = null;

    public static event Action OnGenerationFinished;

    [HideInInspector][SerializeField] private bool isGridGenerated = false;

    private void Awake()
    {
        LevelManager levelManager = LevelManager.Instance;
        if (levelManager != null)
        {
            Generate(LevelManager.Instance.CurrentGrid);
        }
        // Calls the player spawner in debug if the grid is not empty
        else if(isGridGenerated)
        {
            OnGenerationFinished?.Invoke();
        }
    }

public void Generate(TileType[,] grid)
    {
        if (grid == null)
        {
            Debug.LogWarning("PhysicalMazeGenerator: Grid is null.");
            return;
        }

        if (clearPrevious)
            ClearChildren();

        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                TileType tile = grid[x, y];

                // Flip Y to match editor grid
                int flippedY = (height - 1) - y;

                Vector3 basePosition = new Vector3(
                    x * cellSize,
                    0f,
                    flippedY * cellSize
                );

                SpawnTile(tile, basePosition, x, y);
            }
        }

        isGridGenerated = true;

        ColourGroundWithRandomPreset();

        // Calls the Player Spawner
        OnGenerationFinished?.Invoke();
    }



    public void Clear()
    {
        ClearChildren();
        isGridGenerated = false;
    }
    
    // --- HELPER ---

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    private void ColourGroundWithRandomPreset()
    {
        int rndPresetIndex = UnityEngine.Random.Range(0, environmentColors_SO.Presets.Length);
        ColorGroundWithPresetIndex(rndPresetIndex);
    }

    private GameObject GetPrefabForTile(TileType type)
    {
        return type switch
        {
            TileType.Floor => floorPrefab,
            TileType.Wall => wallPrefab,
            TileType.Start => startPrefab,
            TileType.End => endPrefab,
            TileType.Star => starPrefab,
            TileType.Ice => icePrefab,
            _ => null
        };
    }

    private void SpawnTile(TileType tileType, Vector3 basePosition, int x, int y)
    {
        // 1 WALL: no floor, only wall
        if (tileType == TileType.Wall)
        {
            if (wallPrefab == null)
                return;

            GameObject wall = Instantiate(
                wallPrefab,
                basePosition,
                Quaternion.identity,
                transform
            );

            wall.name = $"Wall_{x}_{y}";
            return;
        }

        // 2️ NOT WALL → spawn floor
        SpawnFloor(basePosition, x, y);

        // 3️ Special tiles on top
        GameObject specialPrefab = GetPrefabForTile(tileType);
        if (specialPrefab == null || tileType == TileType.Floor)
            return;
        if (tileType == TileType.Start || tileType == TileType.End)
            basePosition.y = 0.5f;

        GameObject special = Instantiate(
            specialPrefab,
            basePosition,
            Quaternion.identity,
            transform
        );

        special.name = $"{tileType}_{x}_{y}";
    }

    private void SpawnFloor(Vector3 basePosition, int x, int y)
    {
        if (floorPrefab == null)
            return;

        Vector3 floorPosition = new Vector3(
            basePosition.x,
            0,
            basePosition.z
        );

        GameObject floor = Instantiate(
            floorPrefab,
            floorPosition,
            Quaternion.identity,
            transform
        );

        floor.name = $"Floor_{x}_{y}";
    }

    private void ColorGroundWithPresetIndex(int presetIndex)
    {
        material.SetColor("_TopColor", environmentColors_SO.Presets[presetIndex].Top);
        material.SetColor("_RightColor", environmentColors_SO.Presets[presetIndex].Right);
        material.SetColor("_LeftColor", environmentColors_SO.Presets[presetIndex].Left);
        material.SetColor("_FrontColor", environmentColors_SO.Presets[presetIndex].Front);
        emissiveBandMaterial.SetColor("_EmissionColor", environmentColors_SO.Presets[presetIndex].Emissive);
    }
}

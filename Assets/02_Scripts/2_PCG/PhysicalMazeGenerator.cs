using System;
using UnityEngine;

public class PhysicalMazeGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject icePrefab;
    public GameObject movingPlatformPrefabH;
    public GameObject movingPlatformPrefabV;
    public GameObject startPrefab;
    public GameObject endPrefab;
    public GameObject starPrefab;

    [Header("Layout")]
    public float cellSize = 1f;
    public bool clearPrevious = true;

    [Header("Visuals")]
    [SerializeField] private EnvironmentColors_SO environmentColors_SO;
    [SerializeField] public Material material = null;
    [SerializeField] public Material emissiveBandMaterial = null;

    public static event Action OnGenerationFinished;

    [HideInInspector][SerializeField] private bool isGridGenerated = false;

    private int previousRndPreset;

    private void Awake()
    {
        if (LevelManager.Instance != null)
        {
            // Generate the map on Awake
            Generate(LevelManager.Instance.CurrentGrid);
        }
        else if (isGridGenerated)
        {
            // Used by PlayerSpawner to Instantiate
            OnGenerationFinished?.Invoke();
        }
    }


    // ======================================
    // PUBLIC GENERATION METHOD
    // ======================================
    public void Generate(CellData[,] grid)
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
                CellData cell = grid[x, y];

                // Flip Y to match editor visualization
                int flippedY = (height - 1) - y;

                Vector3 basePosition = new Vector3(
                    x * cellSize,
                    0f,
                    flippedY * cellSize
                );

                SpawnCell(cell, basePosition, x, y);
            }
        }

        isGridGenerated = true;

        ApplyRandomEnvironmentColors();

        OnGenerationFinished?.Invoke();
    }

    public void Clear()
    {
        ClearChildren();
        isGridGenerated = false;
    }

    // ======================================
    // CORE SPAWN LOGIC
    // ======================================
    private void SpawnCell(CellData cell, Vector3 basePosition, int x, int y)
    {
        // 1️⃣ Wall overrides everything
        if (cell.isEmpty)
        {
            SpawnWall(basePosition, x, y);
            return;
        }

        // 2️⃣ Ground always exists
        SpawnGround(cell.ground, basePosition, x, y);

        // 3️⃣ Overlay (optional)
        if (cell.overlay != OverlayType.None)
            SpawnOverlay(cell.overlay, basePosition, x, y);
    }

    private void SpawnWall(Vector3 basePosition, int x, int y)
    {
        if (wallPrefab == null) return;

        GameObject wall = Instantiate(
            wallPrefab,
            basePosition,
            Quaternion.identity,
            transform
        );
        wall.name = $"Wall_{x}_{y}";
    }

    private void SpawnGround(GroundType groundType, Vector3 basePosition, int x, int y)
    {
        GameObject prefab = groundType switch
        {
            GroundType.Floor => floorPrefab,
            GroundType.Ice => icePrefab,
            GroundType.MovingPlatformH => movingPlatformPrefabH,
            GroundType.MovingPlatformV => movingPlatformPrefabV,
            _ => null
        };

        if (prefab == null) return;

        GameObject ground = Instantiate(
            prefab,
            basePosition,
            Quaternion.identity,
            transform
        );

        // Set the value of movement so that is it always coherant with the grid's size
        if (groundType == GroundType.MovingPlatformH || groundType == GroundType.MovingPlatformV)
        {
            ground.GetComponent<PlatformMovement>().MovementAmplitude = cellSize;
        }

        ground.name = $"{groundType}_{x}_{y}";
    }

    private void SpawnOverlay(OverlayType overlayType, Vector3 basePosition, int x, int y)
    {
        GameObject prefab = overlayType switch
        {
            OverlayType.Start => startPrefab,
            OverlayType.End => endPrefab,
            OverlayType.Star => starPrefab,
            _ => null
        };

        if (prefab == null) return;

        basePosition.y += GetOverlayHeight(overlayType);

        GameObject overlay = Instantiate(
            prefab,
            basePosition,
            Quaternion.identity,
            transform
        );

        overlay.name = $"{overlayType}_{x}_{y}";
    }

    private float GetOverlayHeight(OverlayType type)
    {
        return type switch
        {
            OverlayType.Start => 1.2f,
            OverlayType.End => 1.2f,
            OverlayType.Star => 0.75f,
            _ => 0f
        };
    }

    // ======================================
    // HELPERS
    // ======================================
    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    private void ApplyRandomEnvironmentColors()
    {
        if (environmentColors_SO == null || material == null || emissiveBandMaterial == null)
            return;

        int rndPresetIndex = UnityEngine.Random.Range(0, environmentColors_SO.Presets.Length);
        while (rndPresetIndex == previousRndPreset)
        {
            rndPresetIndex = UnityEngine.Random.Range(0, environmentColors_SO.Presets.Length);
        }
        previousRndPreset = rndPresetIndex;

        ApplyEnvironmentColors(rndPresetIndex);
    }

    private void ApplyEnvironmentColors(int presetIndex)
    {
        material.SetColor("_TopColor", environmentColors_SO.Presets[presetIndex].Top);
        material.SetColor("_RightColor", environmentColors_SO.Presets[presetIndex].Right);
        material.SetColor("_LeftColor", environmentColors_SO.Presets[presetIndex].Left);
        material.SetColor("_FrontColor", environmentColors_SO.Presets[presetIndex].Front);
        emissiveBandMaterial.SetColor("_EmissionColor", environmentColors_SO.Presets[presetIndex].Emissive);
    }
}
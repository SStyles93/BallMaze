using System;
using System.Collections.Generic;
using UnityEngine;


public class PathGeneratorManager : MonoBehaviour
{
    [Header("Generation Parameters")]
    public GenerationParamameters_SO generationParams;

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject startPrefab;
    public GameObject endPrefab;
    public GameObject starPrefab;

    [Header("EnvironmentColor")]
    public EnvironmentColors_SO environmentColors_SO;
    public Material groundMaterial;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    private PathGenerator generator = new PathGenerator();
    private bool wasStartInstanced = false;

    CellType[,] cells;
    public CellType[,] Cells => cells;

    public static event Action OnGenerationFinished;

    private void Awake()
    {
        GeneratePath();
    }

    private void OnDisable()
    {
        ClearPath();
    }
    private void OnDestroy()
    {
        ClearPath();
    }

    /// <summary>
    /// Generates the path and spawns prefabs in the scene.
    /// </summary>
    [ContextMenu("Generate Path")]
    public void GeneratePath()
    {
        int randomIndex = UnityEngine.Random.Range(0, environmentColors_SO.Presets.Length);
        groundMaterial.SetColor("_TopColor", environmentColors_SO.Presets[randomIndex].Top);
        groundMaterial.SetColor("_RightColor", environmentColors_SO.Presets[randomIndex].Right);
        groundMaterial.SetColor("_LeftColor", environmentColors_SO.Presets[randomIndex].Left);
        groundMaterial.SetColor("_FrontColor", environmentColors_SO.Presets[randomIndex].Front);

        cells = generator.Generate(generationParams);

        GameObject floor = floorPrefab;
        Vector3 scale = floor.transform.localScale;
        scale.y = generationParams.MapDepth;
        int pathWidth = generationParams.PathWidth;
        scale.x = pathWidth;
        scale.z = pathWidth;
        floor.transform.localScale = scale;

        for (int x = 0; x < cells.GetLength(0); x++)
        {
            for (int y = 0; y < cells.GetLength(1); y++)
            {
                switch (cells[x, y])
                {
                    case CellType.Path:
                        spawnedObjects.Add(Instantiate(floor, new Vector3(x, -(generationParams.MapDepth / 2) + 0.5f, y), Quaternion.identity));
                        break;
                    case CellType.Start:
                        if (!wasStartInstanced)
                        {
                            wasStartInstanced = true;
                            // Generate Start Prefab
                            GameObject startTile = Instantiate(startPrefab, new Vector3(x, 1, y), Quaternion.identity);
                            Vector3 startTileScale = startTile.transform.localScale;
                            startTileScale.x = scale.x / 2;
                            startTileScale.z = scale.z / 2;
                            startTile.transform.localScale = startTileScale;

                            spawnedObjects.Add(startTile);
                            spawnedObjects.Add(Instantiate(floor, new Vector3(x, -(generationParams.MapDepth / 2) + 0.5f, y), Quaternion.identity));
                        }
                        break;
                    case CellType.End:
                        // Generate End Prefab
                        GameObject endTile = Instantiate(endPrefab, new Vector3(x, 1, y), Quaternion.identity);
                        Vector3 endTileScale = endTile.transform.localScale;
                        endTileScale.x = scale.x / 2;
                        endTileScale.z = scale.z / 2;
                        endTile.transform.localScale = endTileScale;   
                        spawnedObjects.Add(endTile);
                        spawnedObjects.Add(Instantiate(floor, new Vector3(x, -(generationParams.MapDepth / 2) + 0.5f, y), Quaternion.identity));
                        break;
                    case CellType.Star:
                        spawnedObjects.Add(Instantiate(starPrefab, new Vector3(x, 1, y), Quaternion.identity));
                        spawnedObjects.Add(Instantiate(floor, new Vector3(x, -(generationParams.MapDepth / 2) + 0.5f, y), Quaternion.identity));
                        break;

                }
            }
        }
        OnGenerationFinished?.Invoke();
    }

    /// <summary>
    /// Clears all generated objects from the scene.
    /// </summary>
    [ContextMenu("Clear Path")]
    public void ClearPath()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(obj);
                else
                    Destroy(obj);
#else
                Destroy(obj);
#endif
            }
        }
        spawnedObjects.Clear();
        wasStartInstanced = false;
    }
}

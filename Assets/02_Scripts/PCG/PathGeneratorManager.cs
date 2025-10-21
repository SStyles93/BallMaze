using UnityEngine;
using System.Collections.Generic;

public class PathGeneratorManager : MonoBehaviour
{
    [Header("Generation Parameters")]
    public GenerationParams generationParams;

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject startPrefab;
    public GameObject endPrefab;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    private PathGenerator generator = new PathGenerator();
    private bool wasStartInstanced = false;

    CellType[,] cells;
    public CellType[,] Cells => cells;

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
        cells = generator.Generate(generationParams);

        GameObject floor = floorPrefab;
        Vector3 scale = floor.transform.localScale;
        scale.y = generationParams.MapDepth;
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
                            spawnedObjects.Add(Instantiate(startPrefab, new Vector3(x, 1, y), Quaternion.identity));
                            spawnedObjects.Add(Instantiate(floor, new Vector3(x, -(generationParams.MapDepth / 2) + 0.5f, y), Quaternion.identity));
                        }
                        break;
                    case CellType.End:
                        // Generate End Prefab
                        spawnedObjects.Add(Instantiate(endPrefab, new Vector3(x, 1, y), Quaternion.identity));
                        spawnedObjects.Add(Instantiate(floor, new Vector3(x, -(generationParams.MapDepth / 2) + 0.5f, y), Quaternion.identity));
                        break;

                }
            }
        }
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

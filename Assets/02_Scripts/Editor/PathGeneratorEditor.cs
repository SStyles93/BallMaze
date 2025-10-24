using UnityEngine;
using UnityEditor;

public class PathGeneratorEditor : EditorWindow
{
    // --- Scene Link ---
    private PathGeneratorManager activeManager;
    private PcgData_SO PcgData_SO;

    // --- Visualization ---
    private CellType[,] generatedGrid;
    private Texture2D gridTexture;
    private bool needsRedraw = true;
    private Vector2 scrollPos;

    private readonly Color wallColor = new Color(0.2f, 0.2f, 0.2f);
    private readonly Color pathColor = new Color(0.9f, 0.9f, 0.9f);
    private readonly Color startColor = Color.green;
    private readonly Color endColor = Color.red;


    [MenuItem("Tools/Path Generator")]
    public static void ShowWindow()
    {
        GetWindow<PathGeneratorEditor>("Path Generator");
    }

    private void OnGUI()
    {
        if (activeManager == null) activeManager = GameObject.FindFirstObjectByType<PathGeneratorManager>();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // --- SECTION 1: PARAMETERS ---
        EditorGUILayout.LabelField("Editor Parameters", EditorStyles.boldLabel);
        EditorGUILayout.Space(3);


        EditorGUILayout.LabelField("Map Size", EditorStyles.miniBoldLabel);
        activeManager.generationParams.MaxX = EditorGUILayout.IntSlider("MaxX", activeManager.generationParams.MaxX, 10, 200);
        activeManager.generationParams.MaxZ = EditorGUILayout.IntSlider("MaxZ", activeManager.generationParams.MaxZ, 10, 200);
        activeManager.generationParams.MapDepth = EditorGUILayout.IntSlider("Map Depth", activeManager.generationParams.MapDepth, 1, 100);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Path Setup", EditorStyles.miniBoldLabel);
        activeManager.generationParams.StartPos = EditorGUILayout.Vector2IntField("Start Position", activeManager.generationParams.StartPos);
        activeManager.generationParams.PlaceEndManually = EditorGUILayout.Toggle("Placed End Manually ?", activeManager.generationParams.PlaceEndManually);
        if (activeManager.generationParams.PlaceEndManually)
        {
            activeManager.generationParams.EndPos = EditorGUILayout.Vector2IntField("End Position", activeManager.generationParams.EndPos);
        }
        else
        {
            activeManager.generationParams.MinEndDistance = EditorGUILayout.IntField("MinDistance", activeManager.generationParams.MinEndDistance);
        }
        activeManager.generationParams.Spacing = EditorGUILayout.IntSlider("Path Spacing", activeManager.generationParams.Spacing, 0, 10);
        activeManager.generationParams.PathDensity = EditorGUILayout.FloatField("Path Density (%)", activeManager.generationParams.PathDensity);
        activeManager.generationParams.PathTwistiness = EditorGUILayout.FloatField("Path Twistiness (%)", activeManager.generationParams.PathTwistiness);
        activeManager.generationParams.PathWidth = EditorGUILayout.IntSlider("Path Width", activeManager.generationParams.PathWidth, 1, 10);
        activeManager.generationParams.AllowBranching = EditorGUILayout.Toggle("Allow Branching", activeManager.generationParams.AllowBranching);

        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        activeManager.generationParams.Seed = EditorGUILayout.IntField("Seed (-1 for random)", activeManager.generationParams.Seed);
        if (GUILayout.Button("🎲", GUILayout.Width(30)))
        {
            activeManager.generationParams.Seed = (int)System.DateTime.Now.Ticks;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);
        if (activeManager.generationParams.Seed != -1)
        {
            activeManager.generationParams.currentSeed = EditorGUILayout.IntField("Current Seed", activeManager.generationParams.currentSeed);
        }

        //if (GUILayout.Button("Generate Preview", GUILayout.Height(30)))
        //{
        //    GeneratePreviewData();
        //}

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(5);

        // --- SECTION 2: SCENE LINK ---
        EditorGUILayout.LabelField("Scene Manager Link", EditorStyles.boldLabel);
        activeManager = (PathGeneratorManager)EditorGUILayout.ObjectField("Path Manager", activeManager, typeof(PathGeneratorManager), true);
        PcgData_SO = (PcgData_SO)EditorGUILayout.ObjectField("PCG Data SO", PcgData_SO, typeof(PcgData_SO), false);

        if (activeManager != null)
        {
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Generate In Scene", GUILayout.Height(40)))
            {
                activeManager.ClearPath();
                activeManager.GeneratePath();
                generatedGrid = activeManager.Cells;
                gridTexture = new Texture2D(activeManager.generationParams.MaxX, activeManager.generationParams.MaxZ);
                UpdateTexture();
            }
            GUI.backgroundColor = Color.white;

            if (GUILayout.Button("Clear Scene"))
            {
                activeManager.ClearPath();
                generatedGrid = null;
                gridTexture = null;
            }

            if(GUILayout.Button("Save Params. to SO"))
            {
                PcgData_SO.AddDataParameter(activeManager.generationParams);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Assign a PathGeneratorManager from your scene to enable in-scene generation.", MessageType.Info);
        }

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Generated Map Preview", EditorStyles.boldLabel);

        if (generatedGrid != null)
        {
            if (needsRedraw && gridTexture != null)
                UpdateTexture();

            Rect rect = GUILayoutUtility.GetAspectRect((float)gridTexture.width / gridTexture.height);
            GUI.DrawTexture(rect, gridTexture, ScaleMode.ScaleToFit);
        }

        EditorGUILayout.EndScrollView();
    }

    private void UpdateTexture()
    {
        if (generatedGrid == null) return;

        int width = generatedGrid.GetLength(0);
        int height = generatedGrid.GetLength(1);

        gridTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point
        };

        var pixels = new Color[width * height]; //0->Max-1 (width * height)-1
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                switch (generatedGrid[x, y])
                {
                    case CellType.Path:
                        pixels[y * width + x] = pathColor;
                        break;
                    case CellType.Start:
                        pixels[y * width + x] = startColor;
                        break;
                    case CellType.End:
                        pixels[y * width + x] = endColor;
                        break;
                }
            }
        }

        gridTexture.SetPixels(pixels);
        gridTexture.Apply();
        needsRedraw = false;
    }
}

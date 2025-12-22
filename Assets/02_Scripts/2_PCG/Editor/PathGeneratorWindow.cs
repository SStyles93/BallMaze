using UnityEngine;
using UnityEditor;

public class PathGeneratorWindow : EditorWindow
{
    private GeneratorParameters_SO parameters;
    public LevelDatabase_SO levelDatabase;
    private int levelIndex = 0;
    private TileType[,] grid;
    private int usedSeed;

    private bool showGrid = true;
    private bool showPath = true;
    private bool showStars = true;

    private float cellSize = 20f;

    [MenuItem("Tools/Path Generator")]
    public static void ShowWindow()
    {
        GetWindow<PathGeneratorWindow>("Path Generator");
    }

    private void OnGUI()
    {
        parameters = (GeneratorParameters_SO)EditorGUILayout.ObjectField(
            "Parameters",
            parameters,
            typeof(GeneratorParameters_SO),
            false
        );

        if (parameters == null)
        {
            EditorGUILayout.HelpBox(
                "Assign a GeneratorParameters ScriptableObject.",
                MessageType.Info
            );
            return;
        }

        // --- Level Database GUI ---
        levelDatabase = (LevelDatabase_SO)EditorGUILayout.ObjectField(
            "Level Database",
            levelDatabase,
            typeof(LevelDatabase_SO),
            false
        );

        levelIndex = EditorGUILayout.IntField("Level Index", levelIndex);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Level")) SaveCurrentLevel();
        if (GUILayout.Button("Load Level")) LoadLevel();
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        DrawSection("Grid Settings", ref showGrid, DrawGridSettings);
        DrawSection("Path Settings", ref showPath, DrawPathSettings);
        DrawSection("Stars", ref showStars, DrawStarSettings);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Used Seed", usedSeed.ToString());

        cellSize = EditorGUILayout.Slider("Cell Size", cellSize, 5f, 100f);

        GUILayout.Space(10);
        if (GUILayout.Button("Generate"))
        {
            grid = Generator.GenerateMaze(parameters, out usedSeed);
        }

        GUILayout.Space(20);
        DrawGrid();
    }

    private void DrawSection(string title, ref bool foldout, System.Action content)
    {
        foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, title);
        if (foldout)
        {
            EditorGUI.indentLevel++;
            content.Invoke();
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawGridSettings()
    {
        EditorGUI.BeginChangeCheck();

        parameters.gridWidth =
            EditorGUILayout.IntField("Width", parameters.gridWidth);
        parameters.gridHeight =
            EditorGUILayout.IntField("Height", parameters.gridHeight);

        parameters.randomEnd =
            EditorGUILayout.Toggle("Random End", parameters.randomEnd);

        if (parameters.randomEnd)
        {
            parameters.endMin =
                EditorGUILayout.Vector2IntField("End Min", parameters.endMin);
            parameters.endMax =
                EditorGUILayout.Vector2IntField("End Max", parameters.endMax);
        }
        else
        {
            parameters.fixedEnd =
                EditorGUILayout.Vector2IntField("End", parameters.fixedEnd);
        }

        parameters.inputSeed =
            EditorGUILayout.IntField("Input Seed (-1 = random)", parameters.inputSeed);

        if (EditorGUI.EndChangeCheck())
            Regenerate();
    }


    private void DrawPathSettings()
    {
        EditorGUI.BeginChangeCheck();

        parameters.pathThickness =
            EditorGUILayout.IntSlider("Thickness", parameters.pathThickness, 0, 5);

        parameters.curvePercent =
            EditorGUILayout.IntSlider("Curve %", parameters.curvePercent, 0, 100);

        if (EditorGUI.EndChangeCheck())
            Regenerate();
    }


    private void DrawStarSettings()
    {
        EditorGUI.BeginChangeCheck();

        parameters.starCount =
            EditorGUILayout.IntSlider("Star Count", parameters.starCount, 0, 20);

        parameters.minStarDistance =
            EditorGUILayout.IntSlider("Min Star Distance", parameters.minStarDistance, 1, 10);

        parameters.starsConnectToEnd =
            EditorGUILayout.Toggle("Stars Connect to End", parameters.starsConnectToEnd);

        if (EditorGUI.EndChangeCheck())
            Regenerate();
    }

    private void DrawGrid()
    {
        if (grid == null)
            return;

        Rect rect = GUILayoutUtility.GetRect(
            parameters.gridWidth * cellSize,
            parameters.gridHeight * cellSize
        );

        for (int y = 0; y < parameters.gridHeight; y++)
        {
            for (int x = 0; x < parameters.gridWidth; x++)
            {
                Rect cell = new(
                    rect.x + x * cellSize,
                    rect.y + y * cellSize,
                    cellSize,
                    cellSize
                );

                Color color = grid[x, y] switch
                {
                    TileType.Wall => Color.gray,
                    TileType.Floor => Color.green,
                    TileType.Start => Color.blue,
                    TileType.End => Color.red,
                    TileType.Star => Color.yellow,
                    _ => Color.magenta
                };

                EditorGUI.DrawRect(cell, color);
                Handles.DrawSolidRectangleWithOutline(cell, Color.clear, Color.black);
            }
        }

        HandleGridMouseInput(rect);
    }

    private void Regenerate()
    {
        if (parameters == null)
            return;

        grid = Generator.GenerateMaze(parameters, out usedSeed);
        Repaint();
    }

    private void HandleGridMouseInput(Rect gridRect)
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0) // left click
        {
            Vector2 localMousePos = e.mousePosition - new Vector2(gridRect.x, gridRect.y);

            int x = Mathf.FloorToInt(localMousePos.x / cellSize);
            int y = Mathf.FloorToInt(localMousePos.y / cellSize);

            // Bounds check
            if (x >= 0 && x < parameters.gridWidth && y >= 0 && y < parameters.gridHeight)
            {
                // Prevent changing start tile
                Vector2Int startPos = new Vector2Int(parameters.gridWidth / 2, parameters.gridHeight - 1);
                if (x == startPos.x && y == startPos.y)
                    return;

                // Cycle tile type
                TileType current = grid[x, y];
                TileType next;
                switch (current)
                {
                    case TileType.Floor:
                        next = TileType.Star;
                        break;
                    case TileType.Star:
                        next = TileType.End;
                        break;
                    case TileType.End:
                        next = TileType.Wall;
                        break;
                    case TileType.Wall:
                        next = TileType.Floor;
                        break;
                    default:
                        next = TileType.Floor;
                        break;
                }

                grid[x, y] = next;

                e.Use(); // consume event
                Repaint();
            }
        }
    }

    // --- New Save/Load Methods ---
    private void SaveCurrentLevel()
    {
        if (levelDatabase == null || grid == null) return;

        while (levelDatabase.levels.Count <= levelIndex)
            levelDatabase.levels.Add(new LevelData_SO());

        LevelData_SO data = levelDatabase.levels[levelIndex];

        // Copy all fields from editor window / parameters
        data.gridWidth = parameters.gridWidth;
        data.gridHeight = parameters.gridHeight;
        data.randomEnd = parameters.randomEnd;
        data.fixedEnd = parameters.fixedEnd;
        data.endMin = parameters.endMin;
        data.endMax = parameters.endMax;
        data.inputSeed = parameters.inputSeed;

        data.pathThickness = parameters.pathThickness;
        data.curvePercent = parameters.curvePercent;

        data.minStarDistance = parameters.minStarDistance;
        data.starsConnectToEnd = parameters.starsConnectToEnd;

        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        data.width = width;
        data.height = height;
        // Save grid (flattend grid for serializeation)
        data.gridData = new TileType[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                data.gridData[y * width + x] = grid[x, y];
            }
        }

        data.usedSeed = usedSeed;

        EditorUtility.SetDirty(levelDatabase);
        AssetDatabase.SaveAssets();
        Debug.Log($"Level {levelIndex} saved successfully!");
    }

    private void LoadLevel()
    {
        if (levelDatabase == null || levelDatabase.levels.Count <= levelIndex) return;

        LevelData_SO data = levelDatabase.levels[levelIndex];
        if (data == null) return;

        // Load all fields into parameters for display and regeneration
        parameters.gridWidth = data.gridWidth;
        parameters.gridHeight = data.gridHeight;
        parameters.randomEnd = data.randomEnd;
        parameters.fixedEnd = data.fixedEnd;
        parameters.endMin = data.endMin;
        parameters.endMax = data.endMax;

        
        parameters.inputSeed = data.usedSeed;

        parameters.pathThickness = data.pathThickness;
        parameters.curvePercent = data.curvePercent;

        parameters.minStarDistance = data.minStarDistance;
        parameters.starsConnectToEnd = data.starsConnectToEnd;

        // Load from flattened grid (for serialization)
        grid = new TileType[data.width, data.height];
        for (int y = 0; y < data.height; y++)
        {
            for (int x = 0; x < data.width; x++)
            {
                grid[x, y] = data.gridData[y * data.width + x];
            }
        }

        usedSeed = data.usedSeed;

        Repaint();
        Debug.Log($"Level {levelIndex} loaded successfully!");
    }
}

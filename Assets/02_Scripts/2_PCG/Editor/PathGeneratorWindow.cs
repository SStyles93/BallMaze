using UnityEngine;
using UnityEditor;

public class PathGeneratorWindow : EditorWindow
{
    private GeneratorParameters_SO parameters;
    public LevelDatabase_SO levelDatabase;
    private PhysicalMazeGenerator physicalGenerator;

    private int levelIndex = 0;
    private CellData[,] grid;
    private int usedSeed;

    private int coinsToEarn;

    private bool showGrid = true;
    private bool showPath = true;
    private bool showStars = true;

    private float cellSize = 20f;

    private PaintMode paintMode = PaintMode.Ground;
    private GroundType selectedGround = GroundType.Floor;
    private OverlayType selectedOverlay = OverlayType.None;

    private enum PaintMode
    {
        Ground,
        Overlay
    }

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
            "Level Database", levelDatabase, typeof(LevelDatabase_SO), false);

        physicalGenerator = (PhysicalMazeGenerator)EditorGUILayout.ObjectField(
            "Physical Generator", physicalGenerator, typeof(PhysicalMazeGenerator), true);


        levelIndex = EditorGUILayout.IntField("Level Index", levelIndex);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Level"))
        {
            if (levelDatabase.GetLevelDataAtIndex(levelIndex) != null)
            {
                if (!EditorUtility.DisplayDialog(
                    "Override Level?",
                    $"A level already exists at index {levelIndex}.\nDo you want to override it?",
                    "Override",
                    "Cancel"))
                    return;
            }

            SaveCurrentLevel();
        }
        if (GUILayout.Button("Load Level"))
        {
            LoadLevel();
            if (physicalGenerator != null)
            {
                physicalGenerator.Generate(grid);
            }
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        DrawSection("Grid Settings", ref showGrid, DrawGridSettings);
        DrawSection("Path Settings", ref showPath, DrawPathSettings);
        DrawSection("Stars & Currencies", ref showStars, DrawStarSettings);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Used Seed", usedSeed.ToString());

        cellSize = EditorGUILayout.Slider("Cell Size", cellSize, 5f, 100f);

        GUILayout.Space(10);
        if (GUILayout.Button("Generate"))
        {
            grid = Generator.GenerateMaze(parameters, out usedSeed);

            if (physicalGenerator != null)
                physicalGenerator.Generate(grid);
        }

        if (GUILayout.Button("Clear"))
        {
            grid = null;
            physicalGenerator?.Clear();
        }

        GUILayout.Space(20);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Manual Painting", EditorStyles.boldLabel);

        paintMode = (PaintMode)EditorGUILayout.EnumPopup("Paint Mode", paintMode);
        switch (paintMode)
        {
            case PaintMode.Ground:
                selectedGround = (GroundType)EditorGUILayout.EnumPopup("Ground Type", selectedGround);
                break;

            case PaintMode.Overlay:
                selectedOverlay = (OverlayType)EditorGUILayout.EnumPopup("Overlay Type", selectedOverlay);
                break;
        }

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
            //parameters.endMin =
            //    EditorGUILayout.Vector2IntField("End Min", parameters.endMin);
            parameters.endMaxHeightPercent =
                EditorGUILayout.IntField("End Max Height Percent", parameters.endMaxHeightPercent);
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

        parameters.emptyRatio =
            EditorGUILayout.Slider("Emtpy %", parameters.emptyRatio, 0, 1);

        parameters.iceRatio =
            EditorGUILayout.Slider("Ice %", parameters.iceRatio, 0, 1);

        parameters.movingPlatformRatio =
            EditorGUILayout.Slider("Moving Platform %", parameters.movingPlatformRatio, 0, 1);

        if (EditorGUI.EndChangeCheck())
            Regenerate();
    }
    private void DrawStarSettings()
    {
        EditorGUI.BeginChangeCheck();

        parameters.coinsToEarn =
             EditorGUILayout.IntField("Coin to earn", parameters.coinsToEarn);

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

        float overlayScale = 0.5f;

        int width = parameters.gridWidth;
        int height = parameters.gridHeight;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                ref CellData cell = ref grid[x, y];

                Rect cellRect = new Rect(
                    rect.x + x * cellSize,
                    rect.y + y * cellSize,
                    cellSize,
                    cellSize
                );

                // --- Draw ground ---
                Color groundColor = cell.ground switch
                {
                    GroundType.Floor => Color.green,
                    GroundType.Ice => Color.cyan,
                    GroundType.MovingPlatformH => Color.magenta,
                    GroundType.MovingPlatformV => Color.magenta,
                    GroundType.PlatformSide => Color.magenta * 0.75f,
                    _ => Color.pink
                };

                if (cell.isWall)
                    groundColor = Color.gray;

                // Normal ground / wall
                EditorGUI.DrawRect(cellRect, groundColor);
                Handles.DrawSolidRectangleWithOutline(cellRect, Color.clear, Color.black);


                // --- Draw overlays ---
                if (cell.overlay != OverlayType.None)
                {
                    Color overlayColor = cell.overlay switch
                    {
                        OverlayType.Start => Color.blue,
                        OverlayType.End => Color.red,
                        OverlayType.Star => Color.yellow,
                        _ => Color.white
                    };

                    float overlaySize = cellSize * overlayScale;
                    Rect overlayRect = new Rect(
                        cellRect.x + (cellSize - overlaySize) / 2f,
                        cellRect.y + (cellSize - overlaySize) / 2f,
                        overlaySize,
                        overlaySize
                    );

                    EditorGUI.DrawRect(overlayRect, overlayColor);
                    Handles.DrawSolidRectangleWithOutline(overlayRect, Color.clear, Color.black);
                }
            }
        }

        HandleGridMouseInput(rect);
    }

    private void Regenerate()
    {
        if (parameters == null)
            return;

        grid = Generator.GenerateMaze(parameters, out usedSeed);

        if (physicalGenerator != null)
        {
            physicalGenerator.Generate(grid);
        }

        Repaint();
    }
    private void HandleGridMouseInput(Rect gridRect)
    {
        Event e = Event.current;

        if (e.type != EventType.MouseDown)
            return;

        Vector2 localMousePos = e.mousePosition - new Vector2(gridRect.x, gridRect.y);

        int x = Mathf.FloorToInt(localMousePos.x / cellSize);
        int y = Mathf.FloorToInt(localMousePos.y / cellSize);

        if (!IsInsideGrid(x, y))
            return;

        ref CellData cell = ref grid[x, y];

        // Prevent overwriting Start position
        Vector2Int startPos = new(
            parameters.gridWidth / 2,
            parameters.gridHeight - 1
        );

        if (x == startPos.x && y == startPos.y)
            return;

        // LEFT CLICK → paint
        if (e.button == 0)
        {
            switch (paintMode)
            {
                case PaintMode.Ground:
                    if (paintMode == PaintMode.Ground)
                    {
                        if (selectedGround == GroundType.MovingPlatformH || selectedGround == GroundType.MovingPlatformV)
                        {
                            // Determine orientation
                            bool horizontal = selectedGround == GroundType.MovingPlatformH;

                            // Paint center
                            cell.isWall = false;
                            cell.ground = selectedGround;

                            int width = grid.GetLength(0);
                            int height = grid.GetLength(1);

                            // Paint sides
                            if (horizontal)
                            {
                                if (x > 0 && !IsStartOrEnd(x - 1, y))
                                {
                                    grid[x - 1, y].isWall = false;
                                    grid[x - 1, y].ground = GroundType.PlatformSide;
                                }

                                if (x < width - 1 && !IsStartOrEnd(x + 1, y))
                                {
                                    grid[x + 1, y].isWall = false;
                                    grid[x + 1, y].ground = GroundType.PlatformSide;
                                }
                            }
                            else // vertical
                            {
                                if (y > 0 && !IsStartOrEnd(x, y - 1))
                                {
                                    grid[x, y - 1].isWall = false;
                                    grid[x, y - 1].ground = GroundType.PlatformSide;
                                }

                                if (y < height - 1 && !IsStartOrEnd(x, y + 1))
                                {
                                    grid[x, y + 1].isWall = false;
                                    grid[x, y + 1].ground = GroundType.PlatformSide;
                                }
                            }
                        }
                        else
                        {
                            // normal ground
                            cell.isWall = false;
                            cell.ground = selectedGround;
                        }
                    }
                    break;


                case PaintMode.Overlay:
                    if (!cell.isWall)
                        cell.overlay = selectedOverlay;
                    if (cell.isWall && selectedOverlay == OverlayType.Star)
                        cell.overlay = selectedOverlay;
                    break;
            }
        }

        // RIGHT CLICK → first remove overlay, then wall
        if (e.button == 1)
        {
            if (cell.overlay != OverlayType.None)
            {
                cell.overlay = OverlayType.None; // remove overlay first
            }
            else
            {
                cell.isWall = true;  // then turn into wall
            }

        }
        Repaint();
        e.Use();
    }

    private bool IsInsideGrid(int x, int y)
    {
        return x >= 0 && x < parameters.gridWidth &&
               y >= 0 && y < parameters.gridHeight;
    }

    private bool IsStartOrEnd(int x, int y)
    {
        Vector2Int startPos = new Vector2Int(parameters.gridWidth / 2, parameters.gridHeight - 1);
        Vector2Int endPos = parameters.randomEnd ? new Vector2Int(parameters.gridWidth / 2, 0) : parameters.fixedEnd;

        return (x == startPos.x && y == startPos.y) || (x == endPos.x && y == endPos.y);
    }


    // --- Save/Load Methods ---
    private void SaveCurrentLevel()
    {
        if (levelDatabase == null || grid == null) return;


        LevelData_SO data = new LevelData_SO();

        data.index = levelIndex;

        // Copy all fields from editor window / parameters
        data.gridWidth = parameters.gridWidth;
        data.gridHeight = parameters.gridHeight;
        data.randomEnd = parameters.randomEnd;
        data.fixedEnd = parameters.fixedEnd;
        //data.endMin = //parameters.endMin;
        data.endMinHeightPercent = parameters.endMaxHeightPercent;
        data.inputSeed = parameters.inputSeed;

        data.pathThickness = parameters.pathThickness;
        data.curvePercent = parameters.curvePercent;

        data.coinsToEarn = parameters.coinsToEarn;
        data.minStarDistance = parameters.minStarDistance;
        data.starsConnectToEnd = parameters.starsConnectToEnd;

        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        data.gridWidth = width;
        data.gridHeight = height;
        // Save grid (flattend grid for serializeation)
        data.gridData = new CellData[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                data.gridData[y * width + x] = grid[x, y];
            }
        }

        data.usedSeed = usedSeed;

        levelDatabase.SaveLevelData(data);

        EditorUtility.SetDirty(levelDatabase);
        AssetDatabase.SaveAssets();
        Debug.Log($"Level {levelIndex} saved successfully!");
    }

    private void LoadLevel()
    {
        if (levelDatabase == null) return;

        LevelData_SO data = levelDatabase.GetLevelDataAtIndex(levelIndex);
        if (data == null) return;

        // Load all fields into parameters for display and regeneration
        parameters.gridWidth = data.gridWidth;
        parameters.gridHeight = data.gridHeight;
        parameters.randomEnd = data.randomEnd;
        parameters.fixedEnd = data.fixedEnd;
        parameters.endMaxHeightPercent = data.endMinHeightPercent;


        parameters.inputSeed = data.usedSeed;

        parameters.pathThickness = data.pathThickness;
        parameters.curvePercent = data.curvePercent;

        parameters.coinsToEarn = data.coinsToEarn;
        parameters.minStarDistance = data.minStarDistance;
        parameters.starsConnectToEnd = data.starsConnectToEnd;

        // Load from flattened grid (for serialization)
        grid = new CellData[data.gridWidth, data.gridHeight];
        for (int y = 0; y < data.gridHeight; y++)
        {
            for (int x = 0; x < data.gridWidth; x++)
            {
                grid[x, y] = data.gridData[y * data.gridWidth + x];
            }
        }

        usedSeed = data.usedSeed;

        Repaint();
        Debug.Log($"Level {levelIndex} loaded successfully!");
    }
}

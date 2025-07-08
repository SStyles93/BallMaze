using UnityEditor;
using UnityEngine;

namespace PxP
{
    public enum AlgorithmType
    {
        BSP,
        CellularAutomata,
        DrunkardsWalk,
        Perlin,
        WFC
    }
    namespace Tools
    {
        public class DungeonForgeEditorWindow : EditorWindow
        {
            //Generator
            [SerializeField] private AlgorithmType m_algorithmType = AlgorithmType.Perlin;
            [SerializeField] private GameObject m_terrainPrefab;
            [SerializeField] private Vector3 m_mapCenter = Vector3.zero;
            [SerializeField] private Vector2Int m_mapSize = new Vector2Int(40, 40);
            [SerializeField] private int m_mapHeight = 5;
            [SerializeField] private int m_seed = -1;

            //BSP rules
            [SerializeField] private int m_maxRoomEdgeSize = 10;

            //Automata rules
            [SerializeField] private int m_fillProbability = 45;
            [SerializeField] private int m_iterations = 5;

            // Editor variables
            private Vector2 m_scrollPosition = Vector2.zero;
            private bool m_generatorRules = true;
            private bool m_mapRules = true;


            [MenuItem("Tools/DungeonForge/MapGenerator")]
            static void Init()
            {
                // Get existing open window or if none, make a new one:
                DungeonForgeEditorWindow window = (DungeonForgeEditorWindow)EditorWindow.GetWindow(typeof(DungeonForgeEditorWindow));
                window.Show();
            }

            private void OnGUI()
            {
                // Scroll logic ************************************************************************
                m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition, false, false);
                GUILayout.BeginVertical();
                // *************************************************************************************

                GUILayout.Label(new GUIContent("DungeonForge"), EditorStyles.boldLabel);
                GUILayout.Space(10);

                m_generatorRules = EditorGUILayout.Foldout(m_generatorRules, new GUIContent(
               "Generator Rules",
               "Possibility to change the Type of Algorithm and the Seed used for map generation"));

                if (m_generatorRules)
                {
                    GUILayout.Space(5);
                    EditorGUI.indentLevel++;

                    EditorGUI.BeginChangeCheck();
                    AlgorithmType algorithmType = (AlgorithmType)EditorGUILayout.EnumPopup
                    (
                           new GUIContent
                           (
                               "Generator Type",
                               "Defines which algorithm will be used to create the map." +
                               "\n" +
                               "\n[Binary Space Partitionning (BSP)]" +
                               "\nBinary space partitioning is a method for space partitioning " +
                               "which recursively subdivides a Euclidean space into two convex sets by using hyperplanes as partitions." +
                               "\n" +
                               "\n[Cellular Automata]" +
                               "\nA cellular automaton consists of a regular grid of cells, each in one of a finite number of states, " +
                               "such as on and off (in contrast to a coupled map lattice). " +
                               "The grid can be in any finite number of dimensions. " +
                               "For each cell, a set of cells called its neighborhood is defined relative to the specified cell." +
                               "\n" +
                               "\n[Drunkard's Walk]" +
                               "\nThe drunkard's walk algorithm, also known as a random walk, is a simple algorithm used in procedural generation" +
                               "It works by simulating a path created by a \"drunk\" person stumbling around, leaving a trail of footprints that form a connected path." +
                               "\n" +
                               "\n[Perlin]" +
                               "\nPerlin noise is a type of gradient noise developed by Ken Perlin in 1983. It has many uses, " +
                               "including but not limited to: procedurally generating terrain" +
                               "\n" +
                               "\n[Wave Function Collapse (WFC)]" +
                               "\nWave function collapse, also known as state-vector reduction, is a concept in quantum mechanics where a " +
                               "quantum system in a superposition of multiple states, upon measurement, transitions to a single, definite state"
                            ), m_algorithmType
                    );
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(this, "Modified AlgorithmType");
                        m_algorithmType = algorithmType;
                        EditorUtility.SetDirty(this);
                        Repaint();
                    }

                    EditorGUI.BeginChangeCheck();
                    int seed = EditorGUILayout.IntField(new GUIContent(
                    "Seed",
                    "Seed used to generate maps." +
                    "\nUse [-1] to have a random behaviour or " +
                    $"\n[0 - {int.MaxValue}] for reproductible maps"), m_seed);
                    if (seed < -1 || seed > int.MaxValue)
                    {
                        m_seed = -1;
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(this, "Modified Seed value");
                        m_seed = seed;
                        EditorUtility.SetDirty(this);
                        Repaint();
                    }

                    EditorGUI.indentLevel--;
                }
                GUILayout.Space(10);



                m_mapRules = EditorGUILayout.Foldout(m_mapRules, new GUIContent(
               "Map Rules",
               "Possibility to change the map's position and size"));
                if (m_mapRules)
                {
                    GUILayout.Space(5);
                    EditorGUI.indentLevel++;

                    EditorGUI.BeginChangeCheck();
                    Vector3 mapCenter = EditorGUILayout.Vector3Field(new GUIContent("Map Center", "Defines the center of the generated map"), m_mapCenter);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(this, "Modified Map center");
                        m_mapCenter = mapCenter;
                        EditorUtility.SetDirty(this);
                        Repaint();
                    }
                    //GUILayout.Space(10);


                    EditorGUI.BeginChangeCheck();
                    Vector2Int mapSize = EditorGUILayout.Vector2IntField
                    (new GUIContent("Map Size", "Defines the size of the map used for the generation"), m_mapSize);
                    if (mapSize.x < 1)
                    {
                        m_mapSize.x = 1;
                        Debug.LogWarning("Map width must not be lower than 1");
                    }
                    if (mapSize.y < 1)
                    {
                        m_mapSize.y = 1;
                        Debug.LogWarning("Map lenght must not be lower than 1");
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(this, "Modified Map center");
                        m_mapSize = mapSize;
                        EditorUtility.SetDirty(this);
                        Repaint();
                    }

                    //GUILayout.Space(10);


                    EditorGUI.BeginChangeCheck();
                    int mapHeight = EditorGUILayout.IntField(
                        new GUIContent("Map Height", "Defines the height of the generated map"), m_mapHeight);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (mapHeight < 1)
                        {
                            mapHeight = 1;
                            Debug.LogWarning("Map lenght must not be lower than 1");
                        }
                        Undo.RecordObject(this, "Modified MapHeight");
                        m_mapHeight = mapHeight;
                        EditorUtility.SetDirty(this);
                        Repaint();
                    }

                    switch (m_algorithmType)
                    {
                        case AlgorithmType.BSP:
                            GUILayout.Space(5);
                            EditorGUI.BeginChangeCheck();
                            int roomEdge = EditorGUILayout.IntField(
                                new GUIContent("Max Edge Size", "Defines the maximal size of the edges of the room"), m_maxRoomEdgeSize);
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (roomEdge < 1)
                                {
                                    roomEdge = 1;
                                    Debug.LogWarning("Room edge can not be lower than 1");
                                }
                                Undo.RecordObject(this, $"Modified MaxRoomEdgeSize");
                                m_maxRoomEdgeSize = roomEdge;
                                DungeonForge.Generator.SetBSPBorderSize(m_maxRoomEdgeSize);
                                EditorUtility.SetDirty(this);
                                Repaint();
                            }
                            break;
                        case AlgorithmType.CellularAutomata:
                            GUILayout.Space(5);
                            EditorGUI.BeginChangeCheck();
                            int fillProbability = EditorGUILayout.IntField(
                                new GUIContent("Fill Probability", "Defines the chances for a tile to be filled"), m_fillProbability);
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (fillProbability < 1)
                                {
                                    fillProbability = 1;
                                    Debug.LogWarning("Fill probability can not be lower than 1");
                                }
                                Undo.RecordObject(this, "Modified Fill Probability");
                                m_fillProbability = fillProbability;
                                DungeonForge.Generator.SetAutomataFillProbability(m_fillProbability);
                                EditorUtility.SetDirty(this);
                                Repaint();
                            }
                            EditorGUI.BeginChangeCheck();
                            int iterations = EditorGUILayout.IntField(
                                new GUIContent("Number of Iteration", "Defines the number of time the Cellular Automata will smooth the map"), m_iterations);
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (iterations < 1)
                                {
                                    iterations = 1;
                                    Debug.LogWarning("Room edge can not be lower than 1");
                                }
                                Undo.RecordObject(this, "Modified Iterations");
                                m_iterations = iterations;
                                DungeonForge.Generator.SetAutomataIterations(m_iterations);
                                EditorUtility.SetDirty(this);
                                Repaint();
                            }
                            break;
                        default:
                            break;
                    }

                    EditorGUI.indentLevel--;
                }
                GUILayout.Space(20);


                EditorGUI.BeginChangeCheck();
                GameObject terrainPrefab = EditorGUILayout.ObjectField(new GUIContent("Terrain prefab", "The prefab used to create the phisical terrain"),
                   m_terrainPrefab, typeof(GameObject), false) as GameObject;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, "Terrain Prefab changed");
                    m_terrainPrefab = terrainPrefab;
                    DungeonForge.Generator.SetTerrainPrefab(m_terrainPrefab);
                    EditorUtility.SetDirty(this);
                    Repaint();
                }
                GUILayout.Space(20);



                if (GUILayout.Button("Generate Map"))
                {
                    DungeonForge.Generator.Generate(m_mapSize, (uint)m_mapHeight, m_algorithmType, m_seed, m_mapCenter);
                }
                GUILayout.Space(10);


                // Scroll logic ************************************************************************
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
                // *************************************************************************************
            }
        }
    }
}
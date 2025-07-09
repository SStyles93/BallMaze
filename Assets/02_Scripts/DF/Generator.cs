using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PxP.DungeonForge
{
    public class Generator : MonoBehaviour
    {
        private static Vector3 currentPosition = Vector3.zero;
        private static GameObject m_terrainPrefab;
        private static GameObject[][] m_terrainObjects;
        private static GameObject m_parent;

        private static int m_seed = -1;
        private static AlgorithmType m_algorithmType = AlgorithmType.Perlin;
        private static Vector2Int m_mapSize = new Vector2Int(40, 40);
        private static Vector2 m_mapCenter = new Vector2();
        private static uint m_mapHeight = 1;

        private static int m_maxRoomSize = 10;
        private static int m_iterations = 5;
        private static int m_fillProbability = 55;
        private static float m_fillPercentage = 40.0f;

        #region DungeonForge_DLL
        private static IntPtr generatorInstance;

        [DllImport("DungeonForgeLib.dll")]
        private static extern IntPtr CreateGenerator(uint width, uint height);

        [DllImport("DungeonForgeLib.dll")]
        private static extern void DeleteGenerator(IntPtr generatorInstance);

        [DllImport("DungeonForgeLib.dll")]
        private static extern void Generate(IntPtr generatorInstance);

        [DllImport("DungeonForgeLib.dll")]
        private static extern int GetTile(IntPtr generatorInstance, uint x, uint y);

        [DllImport("DungeonForgeLib.dll")]
        private static extern void SetAlgorithm(IntPtr generatorInstance, int type);

        [DllImport("DungeonForgeLib.dll")]
        private static extern void SetSeed(IntPtr generatorInstance, int seed);

        [DllImport("DungeonForgeLib.dll")]
        private static extern void SetMaxRoomEdgeSize(IntPtr generatorInstance, int maxBorderSize);

        [DllImport("DungeonForgeLib.dll")]
        private static extern void SetFillProbability(IntPtr generatorInstance, int value);

        [DllImport("DungeonForgeLib.dll")]
        private static extern void SetIterations(IntPtr generatorInstance, int value);

        [DllImport("DungeonForgeLib.dll")]
        private static extern void SetFillPercentage(IntPtr generatorInstance, float value);
        #endregion

        #region Public Methods

        /// <summary>
        /// Generates the Map and Physical Map
        /// </summary>
        /// <param name="mapCenter">Center of the map</param>
        /// <param name="mapSize">Size of the map (X/Z axes)</param>
        /// <param name="mapHeight">Height of the map (Y axis)</param>
        public static void Generate()
        {
            Init(m_mapSize);
            SetSeed(generatorInstance, m_seed);
            SetAlgorithm(generatorInstance, (int)m_algorithmType);
            switch (m_algorithmType)
            {
                case AlgorithmType.BSP:
                    SetMaxRoomEdgeSize(generatorInstance, m_maxRoomSize);
                    break;
                case AlgorithmType.CellularAutomata:
                    SetFillProbability(generatorInstance, m_fillProbability);
                    SetIterations(generatorInstance, m_iterations);
                    break;
                case AlgorithmType.DrunkardsWalk:
                    SetFillPercentage(generatorInstance, m_fillPercentage);
                    break;
                default:
                    break;
            }
            Generate(generatorInstance);
            GeneratePhisicalMap(m_mapSize, m_mapHeight, m_mapCenter);
        }

        /// <summary>
        /// Resets all the values of the Generator
        /// </summary>
        public static void ResetValues()
        {
            currentPosition = Vector3.zero;
            m_terrainPrefab = null;
            m_terrainObjects = null;
            m_parent = null;

            m_seed = -1;
            m_algorithmType = AlgorithmType.Perlin;
            m_mapSize = new Vector2Int(40, 40);
            m_mapCenter = new Vector2();
            m_mapHeight = 1;

            m_maxRoomSize = 10;
            m_iterations = 5;
            m_fillProbability = 55;
            m_fillPercentage = 40.0f;
        }

        /// <summary>
        /// Sets the Generator Seed (uses the Mersene-twister engine)
        /// </summary>
        /// <param name="value">Value of the seed</param>
        public static void SetSeed(int value)
        {
            if (value < -1 || value > int.MaxValue)
            {
                Debug.Log($"Value must be set between -1 and {int.MaxValue}");
                return;
            }
            m_seed = value;
        }

        /// <summary>
        /// Sets the algorithm type
        /// </summary>
        /// <param name="type">AlgorithmType</param>
        public static void SetAlgorithmType(AlgorithmType type)
        {
            m_algorithmType = type;
        }

        /// <summary>
        /// Sets the Map Size to the given value
        /// </summary>
        /// <param name="size">Vector2Int size</param>
        public static void SetMapSize(Vector2Int size)
        {
            if (size.x < 1)
            {
                size.x = 1;
                Debug.LogWarning("Map size.x should not be lower than 1");
            }
            if (size.y < 1)
            {
                size.y = 1;
                Debug.LogWarning("Map size.y should not be lower than 1");
            }
            m_mapSize = size;
        }

        /// <summary>
        /// Sets the height of the map
        /// </summary>
        /// <param name="height">uint height</param>
        public static void SetMapHeight(uint height)
        {
            if (height < 1)
            {
                Debug.LogWarning("Map height should not be lower that 1");
            }
            m_mapHeight = height;
        }

        /// <summary>
        /// Sets the center of the map
        /// </summary>
        /// <param name="center">Vector2 center</param>
        public static void SetMapCenter(Vector2 center)
        {
            m_mapCenter = center;
        }

        /// <summary>
        /// Sets the value of fill probability / chances to fill a tile of not
        /// </summary>
        /// <param name="value">Value of fill probability</param>
        public static void SetAutomataFillProbability(int value)
        {
            if (value < 1 || value > 100)
            {
                Debug.Log("Fill probability must be between 1-100");
                return;
            }
            m_fillProbability = value;
        }

        /// <summary>
        /// Set the value of number of iteration used to smooth the map 
        /// </summary>
        /// <param name="value">Number of iteration</param>
        public static void SetAutomataIterations(int value)
        {
            if (value < 1 || value > 100)
            {
                Debug.Log("Automata Iterations must be between 1-100");
                return;
            }
            m_iterations = value;
        }

        /// <summary>
        /// Sets the maximum border size of the rooms in BSP algo
        /// </summary>
        /// <param name="size">Max size of a room</param>
        public static void SetBSPBorderSize(int size)
        {
            if (size < 1)
            {
                Debug.LogWarning("Border size must at least be 1");
                return;
            }
            m_maxRoomSize = size;
        }

        /// <summary>
        /// Sets the % of floor filling for the Drunkard's Walk
        /// </summary>
        /// <param name="value">float Value [1-90]</param>
        public static void SetDrunkardsFillPercentage(float value)
        {
            if (value < 1.0f)
            {
                Debug.LogWarning("Percentage value should not be lower that 1%");
                value = 1.0f;
            }
            if (value > 90)
            {
                Debug.LogWarning("Percentage value should not be higher than 90%");
                value = 90.0f;
            }
            m_fillPercentage = value;
        }

        /// <summary>
        /// Sets the prefab used as "Tiles" 
        /// </summary>
        /// <param name="prefab">The prefab</param>
        public static void SetTerrainPrefab(GameObject prefab)
        {
            m_terrainPrefab = prefab;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Generated the phisical map with the data from the generator
        /// </summary>
        /// <param name="mapCenter">Center of the Map</param>
        /// <param name="mapSize">Size (X/Z axis) of the Map</param>
        /// <param name="mapHeight">Height (Y axis) of the map</param>
        private static void GeneratePhisicalMap(Vector2Int mapSize, uint mapHeight, Vector3 mapCenter = default)
        {
            if (!ValuesAreCorrect(mapSize, mapHeight)) return;
            if (m_terrainPrefab == null)
            {
                Debug.LogWarning("There is not GameObject contained in the \"Terrain prefab\" field\n" +
                    "In order for the script to work you need to input a GameObject");
                return;
            }
            if (m_parent != null)
            {
                DestroyImmediate(m_parent);
            }

            m_parent = new GameObject("Terrain Tiles");

            Vector3 physicalPosition = SetInstancePosition(mapCenter, mapSize);

            currentPosition = physicalPosition;

            m_terrainObjects = new GameObject[mapSize.x][];
            for (uint x = 0; x < mapSize.x; x++)
            {
                m_terrainObjects[x] = new GameObject[mapSize.y];
                for (uint y = 0; y < mapSize.y; y++)
                {
                    if (GetTile(generatorInstance, x, y) == 0)
                    {
                        m_terrainObjects[x][y] = Instantiate(m_terrainPrefab, currentPosition, Quaternion.identity, m_parent.transform);
                        for (uint z = 0; z < mapHeight - 1; z++)
                        {
                            currentPosition.y--;
                            Instantiate(m_terrainPrefab, currentPosition, Quaternion.identity, m_parent.transform);
                        }
                        currentPosition.y = physicalPosition.y;
                    }
                    currentPosition.z++;
                }
                currentPosition.x++;
                currentPosition.z = physicalPosition.z;
            }
        }

        /// <summary>
        /// Initializes the map generator
        /// </summary>
        /// <param name="mapWidth">Width of the map</param>
        /// <param name="mapHeight">Height of the map</param>
        private static void Init(int mapWidth = 1, int mapHeight = 1)
        {
            if (mapWidth < 1)
            {
                mapWidth = 1;
                Debug.LogWarning("Map Width cannot be smaller than 1");
            }
            if (mapHeight < 1)
            {
                mapHeight = 1;
                Debug.LogWarning("Map Height cannot be smaller than 1");
            }


            generatorInstance = CreateGenerator((uint)mapWidth, (uint)mapHeight);
        }

        /// <summary>
        /// Initializes the map generator
        /// </summary>
        /// <param name="mapSize">Map width and height</param>
        private static void Init(Vector2Int mapSize = default)
        {
            if (mapSize == default)
            {
                Debug.LogWarning("Map Size cannot be smaller than (1,1)");
                return;
            }
            generatorInstance = CreateGenerator((uint)mapSize.x, (uint)mapSize.y);
        }

        /// <summary>
        /// Sets the instance position according to the Center and the MapSize
        /// </summary>
        /// <param name="mapCenter">Center of the Map</param>
        private static Vector3 SetInstancePosition(Vector3 mapCenter, Vector2Int mapSize)
        {
            Vector3 tmpVec = new Vector3();
            tmpVec = mapCenter + new Vector3
            (
                -mapSize.x / 2 + 0.5f,
                0,
                -mapSize.y / 2 + 0.5f
            );
            return tmpVec;
        }

        /// <summary>
        /// Returns true if all values are in the correct bound
        /// </summary>
        /// <param name="mapSize">Map Size</param>
        /// <param name="mapHeight">Map Height</param>
        /// <returns>Bool</returns>
        private static bool ValuesAreCorrect(Vector2Int mapSize, uint mapHeight)
        {
            if (mapSize == default)
            {
                Debug.LogWarning("Map Size cannot be smaller than (1,1)");
                return false;
            }
            if (mapHeight < 1)
            {
                Debug.LogWarning("Map Height cannot be smaller than 1");
                return false;
            }
            return true;
        }

        #endregion


        /// <summary>
        /// Savec the currently displayed map
        /// </summary>
        /// <param name="invertLines">if true inverts the Y writing order (starts at the bottom-left)(</param>
        public static void SaveMap(bool invertLines = false, bool isDataPersistent = false)
        {
            SaveData mapData = new SaveData();
            mapData.mapSize = m_mapSize;
            mapData.mapHeight = m_mapHeight;

            string mapString = "";
            for (uint x = 0; x < m_mapSize.x; x++)
            {
                string mapLine = "\n";

                for (uint y = 0; y < m_mapSize.y; y++)
                {
                    if (GetTile(generatorInstance, x, y) == 0)
                    {
                        mapLine += "0";
                    }
                    else
                    {
                        mapLine += "1";
                    }
                }
                if (invertLines)
                    mapString = mapLine + mapString; //Inverts writing order of the lines to respect Unity map order X+/Y+
                else
                    mapString += mapLine; //Writes lines normally
            }
            Debug.Log(mapString);
            mapData.mapTiles = mapString;
            string json = JsonUtility.ToJson(mapData);


            string path; 
            path = isDataPersistent ? Application.persistentDataPath : Application.dataPath;
            File.WriteAllText(path + "/SavedMap.json", json);
            
        }
        
        /// <summary>
        /// Loads a JSON file and reinterprets it in an instantiated 3D map
        /// </summary>
        public static void LoadMap(bool wasDataPersistent = false)
        {
            string path;
            path = wasDataPersistent ? Application.persistentDataPath : Application.dataPath;
            path += "/SavedMap.json";
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                SaveData mapData = JsonUtility.FromJson<SaveData>(json);

                m_mapSize = mapData.mapSize;
                m_mapHeight = mapData.mapHeight;
                string mapString = mapData.mapTiles;

                m_parent = new GameObject("Terrain Tiles");
                Vector3 physicalPosition = SetInstancePosition(m_mapCenter, m_mapSize);
                currentPosition = physicalPosition;

                m_terrainObjects = new GameObject[m_mapSize.x][];
                for (int x = 0; x < m_mapSize.x; x++)
                {
                    mapString = mapString.Substring(1, mapString.Length-1);
                    m_terrainObjects[x] = new GameObject[m_mapSize.y];
                    for (int y = 0; y < m_mapSize.y; y++)
                    {
                        if (mapString[0] == '0')
                        {
                            m_terrainObjects[x][y] = Instantiate(m_terrainPrefab, currentPosition, Quaternion.identity, m_parent.transform);
                            for (int z = 0; z < m_mapHeight - 1; z++)
                            {
                                currentPosition.y--;
                                Instantiate(m_terrainPrefab, currentPosition, Quaternion.identity, m_parent.transform);
                            }
                            currentPosition.y = physicalPosition.y;
                        }
                        
                        mapString = mapString.Substring(1, mapString.Length - 1);
                        currentPosition.z++;
                    }
                    currentPosition.x++;
                    currentPosition.z = physicalPosition.z;
                }
            }
        }
    }

    [System.Serializable]
    class SaveData
    {
        public Vector2Int mapSize;
        public uint mapHeight;
        public string mapTiles;
    }
}
using System;
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
        #endregion

        #region Public Methods

        /// <summary>
        /// Generates the Map and Physical Map
        /// </summary>
        /// <param name="mapCenter">Center of the map</param>
        /// <param name="mapSize">Size of the map (X/Z axes)</param>
        /// <param name="mapHeight">Height of the map (Y axis)</param>
        public static void Generate(Vector2Int mapSize, uint mapHeight, AlgorithmType algorithmType, int seed = -1, Vector3 mapCenter = default)
        {
            Init(mapSize);
            SetSeed(generatorInstance, seed);
            SetAlgorithm(generatorInstance, (int)algorithmType);
            Generate(generatorInstance);
            GeneratePhisicalMap(mapSize, mapHeight, mapCenter);
        }

        /// <summary>
        /// Sets the value of fill probability / chances to fill a tile of not
        /// </summary>
        /// <param name="value">Value of fill probability</param>
        public static void SetAutomataFillProbability(int value)
        {
            SetFillProbability(generatorInstance, value);
        }

        /// <summary>
        /// Set the value of number of iteration used to smooth the map 
        /// </summary>
        /// <param name="value">Number of iteration</param>
        public static void SetAutomataIterations(int value)
        {
            SetIterations(generatorInstance, value);
        }

        /// <summary>
        /// Sets the maximum border size of the rooms in BSP algo
        /// </summary>
        /// <param name="size">Max size of a room</param>
        public static void SetBSPBorderSize(int size)
        {
            SetMaxRoomEdgeSize(generatorInstance, size);
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
            if(!ValuesAreCorrect(mapSize, mapHeight)) return;
            if (m_terrainPrefab == null)
            {
                Debug.LogWarning("There is not GameObject contained in the \"Terrain prefab\" field\n" +
                    "In order for the script to work you need to input a GameObject");
                return;
            }
            if(m_parent != null)
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
                        for (uint z = 0; z < mapHeight-1; z++)
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
    }
}
using NUnit.Framework.Internal;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class TestPluginWrapper : MonoBehaviour
{
    [SerializeField] Vector2Int mapSize = new Vector2Int(0, 0);

    //private IntPtr mapInstance;
    private IntPtr generatorInstance;

    //[DllImport("DungeonForgeLib.dll")]
    //private static extern IntPtr CreateMap(uint width, uint height);

    //[DllImport("DungeonForgeLib.dll")]
    //private static extern void DeleteMap(IntPtr mapInstance);

    //[DllImport("DungeonForgeLib.dll")]
    //private static extern void SetTile(IntPtr mapInstance, uint x, uint y, int value);

    //[DllImport("DungeonForgeLib.dll")]
    //private static extern int GetTile(IntPtr mapInstance, uint x, uint y);

    //[DllImport("DungeonForgeLib.dll")]
    //private static extern int GetWidth(IntPtr mapInstance);

    //[DllImport("DungeonForgeLib.dll")]
    //private static extern int GetHeight(IntPtr mapInstance);

    [DllImport("DungeonForgeLib.dll")]
    private static extern IntPtr CreateGenerator(uint width, uint height);

    [DllImport("DungeonForgeLib.dll")]
    private static extern void DeleteGenerator(IntPtr generatorInstance);

    [DllImport("DungeonForgeLib.dll")]
    private static extern void Generate(IntPtr generatorInstance);

    [DllImport("DungeonForgeLib.dll")]
    private static extern IntPtr GetMap(IntPtr generatorInstance);

    [DllImport("DungeonForgeLib.dll")]
    private static extern int GetTile(IntPtr generatorInstance, uint x, uint y);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //mapInstance = CreateMap((uint)mapSize.x, (uint)mapSize.y);

        //SetTile(mapInstance, 1, 1, 0);
        //Debug.Log(GetTile(mapInstance, 1, 1));

        //SetTile(mapInstance, 1, 1, 1);
        //Debug.Log(GetTile(mapInstance, 1, 1));

        //Debug.Log(GetWidth(mapInstance));
        //Debug.Log(GetHeight(mapInstance));

        generatorInstance = CreateGenerator((uint)mapSize.x, (uint)mapSize.y);
        Generate(generatorInstance);

        string line = "";
        for (uint y = 0; y < mapSize.y; y++)
        {
            for (uint x = 0; x < mapSize.x; x++)
            {
                line += GetTile(generatorInstance, x, y);
            }
            line += "\n";
        }
        Debug.Log(line);
    }
    private void OnDestroy()
    {
        if (generatorInstance != IntPtr.Zero)
        {
            DeleteGenerator(generatorInstance);
            generatorInstance = IntPtr.Zero;
        }
    }
}

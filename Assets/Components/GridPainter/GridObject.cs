using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using JsonReader = Newtonsoft.Json.JsonReader;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
using JsonWriter = Newtonsoft.Json.JsonWriter;

[Serializable]
[CreateAssetMenu(fileName = "TileSettings", menuName = "Grid/Level")]
public class GridObject : ScriptableObject
{
    public string fileName = "Level 1";
    public GridTileStruct[,] tiles;
    
    public void Save()
    {
        string jsonArray = GridTileArrayConverter.Serialize(tiles);
        // string jsonArray = JsonUtility.ToJson(new GridLevel(tiles), true);
        string path = Path.Combine(Path.Combine(Application.dataPath, "SaveFiles"), fileName);
        File.WriteAllText(path, jsonArray);
        Debug.Log($"Successfully saved grid tiles {jsonArray}");
    }

    public void Load()
    {
        string path = Path.Combine(Path.Combine(Application.dataPath, "SaveFiles"), fileName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            tiles = GridTileArrayConverter.Deserialize(json);
            // tiles = JsonUtility.FromJson<GridLevel>(json).tiles;
        }
        else
        {
            Debug.LogWarning("Save file not found");
        }
    }
}

public class GridLevel
{
    public GridTileStruct[,] tiles;
    public GridLevel(GridTileStruct[,] tiles)
    {
        this.tiles = tiles;
    }
}

public struct GridTileStructJson
{
    public int tileID;
    public int version;

    public GridTileStructJson(int tileID, int version)
    {
        this.tileID = tileID;
        this.version = version;
    }
}

[Serializable]
public struct FlatGridTileWrapper
{
    public int[] flatGridTileID; // Flattened 2D array
    public int[] flatGridVersion; // Flattened 2D array
    public int rows;
    public int cols;
}

public static class GridTileArrayConverter
{
    public static string Serialize(GridTileStruct[,] array)
    {
        var wrapper = new FlatGridTileWrapper
        {
            flatGridTileID = new int[array.GetLength(0) * array.GetLength(1)],
            flatGridVersion = new int[array.GetLength(0) * array.GetLength(1)],
            rows = array.GetLength(0),
            cols = array.GetLength(1),
        };

        int counter = 0;
        for (int i = 0; i < wrapper.rows; i++)
        for (int j = 0; j < wrapper.cols; j++)
        {
            GridTileStructJson gridTileStructJson = new GridTileStructJson((int)array[i, j].tileID, array[i, j].version);
            wrapper.flatGridTileID[counter] = gridTileStructJson.tileID;
            wrapper.flatGridVersion[counter] = gridTileStructJson.version;
            counter++;
        }

        return JsonUtility.ToJson(wrapper);
    }

    public static GridTileStruct[,] Deserialize(string json)
    {
        var wrapper = JsonUtility.FromJson<FlatGridTileWrapper>(json);

        if (wrapper.flatGridTileID == null || wrapper.flatGridTileID.Length == 0)
            return new GridTileStruct[0, 0];

        var array = new GridTileStruct[wrapper.rows, wrapper.cols];
        int index = 0;

        for (int i = 0; i < wrapper.rows; i++)
        for (int j = 0; j < wrapper.cols; j++)
        {
            GridTileStruct gridTileStructJson = new GridTileStruct((EntityTileID)wrapper.flatGridTileID[index], wrapper.flatGridVersion[index]);
            array[i, j] = gridTileStructJson;
            index++;
        }

        return array;
    }
}







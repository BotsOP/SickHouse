using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "TileSettings", menuName = "Grid/Level")]
public class GridObject : ScriptableObject
{
    public static void Save(GridTileStruct[,] tiles, string fileName)
    {
        string jsonArray = GridTileArrayConverter.Serialize(tiles);
        string path = Path.Combine(Path.Combine(Application.dataPath, "Resources"), fileName + ".json");
        File.WriteAllText(path, jsonArray);
        Debug.Log($"Successfully saved grid tiles {jsonArray}");
    }

    public static GridTileStruct[,] Load(TextAsset json)
    {
        return GridTileArrayConverter.Deserialize(json);
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

    public static GridTileStruct[,] Deserialize(TextAsset json)
    {
        var wrapper = JsonUtility.FromJson<FlatGridTileWrapper>(json.text);

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







using System;
using System.IO;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "TileSettings", menuName = "Grid/Level")]
public class GridObject : ScriptableObject
{
    public string fileName = "Level 1";
    public int[] tiles;
    
    public void Save()
    {
        string jsonArray = JsonUtility.ToJson(new GridLevel(tiles), true);
        string path = Path.Combine(Path.Combine(Application.dataPath, "SaveFiles"), fileName);
        File.WriteAllText(path, jsonArray);
        Debug.Log($"Successfully saved grid tiles");
    }

    public void Load()
    {
        string path = Path.Combine(Path.Combine(Application.dataPath, "SaveFiles"), fileName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            
            tiles = JsonUtility.FromJson<GridLevel>(json).tiles;
        }
        else
        {
            Debug.LogWarning("Save file not found");
        }
    }
}

public class GridLevel
{
    public int[] tiles;
    public GridLevel(int[] tiles)
    {
        this.tiles = tiles;
    }
}

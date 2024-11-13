using System;
using System.IO;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "TileSettings", menuName = "Grid/Level")]
public class GridObject : ScriptableObject
{
    public string fileName = "Level 1";
    public int[,] tiles;
    
    public void Save()
    {
        string jsonArray = ArraySerializer.SaveArray(tiles);
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
            tiles = ArraySerializer.LoadArray(json);
        }
        else
        {
            Debug.LogWarning("Save file not found");
        }
    }
}

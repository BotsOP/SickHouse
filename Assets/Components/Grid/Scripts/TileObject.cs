using System;
using UnityEngine;


public enum TileID
{
    GRASS,
    DIRT,
}

[CreateAssetMenu(fileName = "TileSettings", menuName = "Tiles/Settings")]
public class TileObject : ScriptableObject
{
    public Tile[] tiles;
}

[Serializable]
public struct Tile
{
    public Mesh mesh;
    public Material material;
    public TileID tileID;
}

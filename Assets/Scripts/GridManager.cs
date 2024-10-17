using System;
using UnityEngine;

public enum TileID
{
    GRASS,
    DIRT,
}

public class GridManager : MonoBehaviour
{
    [SerializeField] private float tileSize;
    [SerializeField] private Tile[][] tiles;

    public bool GetTile(Vector3 worldPos, out Tile tile)
    {
        tile = new Tile();
        return false;
    }
}

[Serializable]
public struct Tile
{
    public TileID tileID;
}


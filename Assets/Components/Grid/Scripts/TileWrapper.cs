using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum EntityTileID
{
    //Tiles
    EMPTY,
    
    TREE,
    DAMM,
    CLIFF,
    DIRT,
    GRASS,
    WATER,
    PAVEMENT,
}

[Serializable]
public struct GridTileStruct
{
    public EntityTileID tileID;
    public int version;

    public GridTileStruct(EntityTileID tileID, int version)
    {
        this.tileID = tileID;
        this.version = version;
    }
}

[CreateAssetMenu(fileName = "Tile", menuName = "Tiles/TileWrapper")]
public class TileWrapper : ScriptableObject
{
    public int order;
    public TileRenderSettings[] renderSettings;
    public TileGameSettings TileGameSettings;
}
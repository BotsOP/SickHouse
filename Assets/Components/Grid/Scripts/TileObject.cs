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
}

[Serializable]
public enum FloorTileID
{
    //Floor Tiles
    EMPTY,
    DIRT,
    GRASS,
    WATER,
    PAVEMENT,
}

[CreateAssetMenu(fileName = "TileWrapper", menuName = "Tiles/TileWrapper")]
public class TileWrapper : ScriptableObject
{
    public FloorTileID floorID;
    public TileRenderSettings renderSettings;
    public TileGameSettings TileGameSettings;
}

[CreateAssetMenu(fileName = "FloorTileWrapper", menuName = "Tiles/FloorTileWrapper")]
public class FloorTileWrapper : ScriptableObject
{
    public TileGameSettings TileGameSettings;
}

[CreateAssetMenu(fileName = "TileGameSettings", menuName = "Tiles/TileGameSettings")]
public class TileGameSettings : ScriptableObject
{
    public int appleCost;

    [Header("Selection")]
    public AreaSelection[] selection;
    [Header("Placement Constraints")]
    public AreaConstraint[] placementConstraints;
    [Header("Placement Requirement")]
    public AreaRequirement[] placementRequirements;
}

[CreateAssetMenu(fileName = "TileRenderSettings", menuName = "Tiles/Rendering")]
public class TileRenderSettings : ScriptableObject
{
    public Material material;
    public TextureWtihReference[] textures;
    public Mesh mesh;
}

[Serializable]
public struct AreaSelection
{
    public EntityTileID entityTileID;
    public float score;
    public SelectionBox[] selectionBoxes;
}

[Serializable]
public struct AreaConstraint
{
    public EntityTileID[] tileIDs;
    public FloorTileID[] floorTileIDs;
    public SelectionBox[] selectionBoxes;
}

[Serializable]
public struct AreaRequirement
{
    public EntityTileID[] tileIDs;
    public FloorTileID[] floorTileIDs;
    public int amountRequiredTiles;
    public SelectionBox[] selectionBoxes;
}

[Serializable]
public struct SelectionBox
{
    public Vector2Int position;
    public Vector2Int size;
}

[Serializable]
public struct TextureWtihReference
{
    public string textureName;
    public Texture2D texture;
}
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum TileID
{
    DIRT,
    GRASS,
    TREE,
    WATER,
    WALL,
    DAMM,
    DAMM_WATER,
}

public enum TileFloorID
{
    DIRT,
    GRASS,
    WATER,
    EMPTY,
}

[CreateAssetMenu(fileName = "TileSettings", menuName = "Tiles/Settings")]
public class TileObject : ScriptableObject
{
    public TileSettings[] tileSettings;
}

[Serializable]
public struct TileSettings
{
    [Header("Tile Settings")]
    public Mesh mesh;
    public Material material;
    public Texture2D texture;
    public TileID tileID;
    public TileFloorID floorID;
    public int appleCost;
    [Header("Selection")]
    public AreaSelection[] selection;
    [Header("Placement Constraints")]
    public AreaConstraint[] placementConstraints;
    [Header("Placement Requirement")]
    public AreaRequirement[] placementRequirements;
}


[Serializable]
public struct AreaSelection
{
    public TileID tileID;
    public float score;
    public SelectionBox[] selectionBoxes;
}

[Serializable]
public struct AreaConstraint
{
    public TileID[] tileID;
    public SelectionBox[] selectionBoxes;
}

[Serializable]
public struct AreaRequirement
{
    public TileID[] tileID;
    public int amountRequiredTiles;
    public SelectionBox[] selectionBoxes;
}

[Serializable]
public struct SelectionBox
{
    public Vector2Int position;
    public Vector2Int size;
}
    
// [Serializable]
// public struct SelectionSphere
// {
//     public Vector2Int position;
//     public int size;
// }

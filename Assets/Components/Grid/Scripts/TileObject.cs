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
}

[CreateAssetMenu(fileName = "TileSettings", menuName = "Tiles/Settings")]
public class TileObject : ScriptableObject
{
    public Tile[] tiles;
}

[Serializable]
public struct Tile
{
    [Header("Tile Settings")]
    public Mesh mesh;
    public Texture2D texture;
    public TileID tileID;
    [Header("Selection")]
    public AreaSelection[] selection;
    [Header("Placement Constraints")]
    public AreaRequirement[] placementConstraints;
    [Header("Placement Requirement")]
    public AreaRequirement[] placementRequirements;
}


[Serializable]
public struct AreaSelection
{
    public TileID tileID;
    public float score;
    public SelectionBox[] selectionBoxes;
    public SelectionSphere[] selectionSpheres;
}

[Serializable]
public struct AreaRequirement
{
    public TileID[] tileID;
    public SelectionBox[] selectionBoxes;
    public SelectionSphere[] selectionSpheres;
}

[Serializable]
public struct SelectionBox
{
    public Vector2Int position;
    public Vector2Int size;
}
    
[Serializable]
public struct SelectionSphere
{
    public Vector2Int position;
    public int size;
}

using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class TileGameSettings
{
    public int appleCost;

    public AreaSelection[] selection;
    public AreaConstraint[] placementConstraints;
    public AreaRequirement[] placementRequirements;
}

[Serializable]
public struct AreaSelection
{
    // public EntityTileID entityTileID;
    // public float score;
    public SelectionBox[] selectionBoxes;
}

[Serializable]
public struct AreaConstraint
{
    public EntityTileID[] tileIDs;
    public SelectionBox[] selectionBoxes;
}

[Serializable]
public struct AreaRequirement
{
    public EntityTileID[] tileIDs;
    public int amountRequiredTiles;
    public SelectionBox[] selectionBoxes;
}

[Serializable]
public struct SelectionBox
{
    public Vector2Int position;
    public Vector2Int size;
}
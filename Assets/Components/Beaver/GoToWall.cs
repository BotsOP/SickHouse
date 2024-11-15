using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using UnityEngine;



[System.Serializable]
public class GridTile
{
    public TileID[] tilesFlattened;
    public GridTile()
    {
    }
}

[System.Serializable]
public class SharedTiles : SharedVariable<GridTile>
{
    public static implicit operator SharedTiles(GridTile value) { return new SharedTiles { Value = value }; }
}

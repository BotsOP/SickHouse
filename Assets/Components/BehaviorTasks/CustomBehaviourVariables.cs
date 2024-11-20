using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using UnityEngine;

[System.Serializable]
public class GridInfoClass
{
    public TileID[] tilesFlattened;
    public List<List<Matrix4x4>> matricesList;
    public int gridWidth;
    public int gridHeight;
    public float tileSize;
}
[System.Serializable]
public class GridInfo : SharedVariable<GridInfoClass>
{
    public static implicit operator GridInfo(GridInfoClass value) { return new GridInfo { Value = value }; }
}

[System.Serializable]
public class DammArrayClass
{
    public Damm[] dammArray;
}

[System.Serializable]
public class Damm
{
    public int amountBeavorsWorking;
    public float progress;
    public bool buildDamm;
}
[System.Serializable]
public class DammArray : SharedVariable<DammArrayClass>
{
    public static implicit operator DammArray(DammArrayClass value) { return new DammArray { Value = value }; }
}
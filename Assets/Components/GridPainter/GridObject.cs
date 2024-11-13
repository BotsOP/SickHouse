using UnityEngine;

[CreateAssetMenu(fileName = "TileSettings", menuName = "Grid/Level")]
public class GridObject : ScriptableObject
{
    public TileID[,] tiles;
}

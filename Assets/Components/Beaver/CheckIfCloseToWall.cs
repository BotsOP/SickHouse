using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;


public class CheckIfCloseToWall : Conditional
{
    public GridInfo gridInfo;
    public DammArray dammArray;
    public SharedInt amountTilesDammFromWall;
    public SharedInt amountTilesTooCloseToWall;
    public SharedInt currentDammIndex;
    public override TaskStatus OnUpdate()
    {
        Vector2Int posIndex = GetTile(currentDammIndex.Value);
        int index = IndexPosToIndex(posIndex + new Vector2Int(0, amountTilesTooCloseToWall.Value));
        if (gridInfo.Value.floorTileIDs[index] == FloorTileID.PAVEMENT)
        {
            amountTilesDammFromWall.Value++;
            dammArray.Value.dammArray[currentDammIndex.Value].amountBeavorsWorking--;
            return TaskStatus.Failure;
        }
        return TaskStatus.Running;
    }
    
    private Vector2Int GetTile(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt((worldPos.x / gridInfo.Value.tileSize) + (gridInfo.Value.gridWidth * gridInfo.Value.tileSize / 2f)), Mathf.RoundToInt((worldPos.z / gridInfo.Value.tileSize) + (gridInfo.Value.gridHeight * gridInfo.Value.tileSize / 2f)));
    }

    private Vector2Int GetTile(int index)
    {
        return new Vector2Int(index / gridInfo.Value.gridWidth, index % gridInfo.Value.gridHeight);
    }
    
    private Vector3 GetPosition(Vector2Int index)
    {
        return new Vector3(Mathf.RoundToInt((index.x * gridInfo.Value.tileSize) - (gridInfo.Value.gridWidth * gridInfo.Value.tileSize / 2f)), 0, Mathf.RoundToInt((index.y * gridInfo.Value.tileSize) - (gridInfo.Value.gridHeight * gridInfo.Value.tileSize / 2f)));
    }
    
    private int IndexPosToIndex(Vector2Int posIndex)
    {
        return posIndex.x * gridInfo.Value.gridWidth + posIndex.y % gridInfo.Value.gridHeight;
    }
}
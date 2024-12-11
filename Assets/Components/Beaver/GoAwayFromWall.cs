using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;


public class GoAwayFromWall : Conditional
{
    public GridInfo gridInfo;
    public SharedVector3 targetPosition;
    public DammArray dammArray;
    public SharedInt currentDammIndex;
    public override TaskStatus OnUpdate()
    {
        for (int i = 0; i < gridInfo.Value.waterSpots.Count; i++)
        {
            Vector2Int posIndex = gridInfo.Value.waterSpots[i];
            int newIndex = IndexPosToIndex(posIndex);
            if (dammArray.Value.dammArray[newIndex].amountBeavorsWorking < 3 && !dammArray.Value.dammArray[newIndex].buildDamm)
            {
                dammArray.Value.dammArray[newIndex].amountBeavorsWorking++;
                targetPosition.Value = GetPosition(posIndex);
                currentDammIndex.Value = newIndex;
                return TaskStatus.Success;
            }
        }
        return TaskStatus.Failure;
    }
    
    private Vector2Int GetTile(Matrix4x4 matrix)
    {
        return GetTile(new Vector3(matrix.GetRow(0).w, 0, matrix.GetRow(2).w));
    }
    private Vector2Int GetTile(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt((worldPos.x / gridInfo.Value.tileSize) + (gridInfo.Value.gridWidth * gridInfo.Value.tileSize / 2f)), Mathf.RoundToInt((worldPos.z / gridInfo.Value.tileSize) + (gridInfo.Value.gridHeight * gridInfo.Value.tileSize / 2f)));
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
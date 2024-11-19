using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Managers;
using UnityEngine;
using EventType = Managers.EventType;


public class BuildDamm : Conditional
{
    public GridInfo gridInfo;
    public SharedVector3 targetPosition;
    public DammArray dammArray;
    public SharedInt currentDammIndex;
    public SharedFloat dammBuildProgress;
    
    public override TaskStatus OnUpdate()
    {
        if (dammArray.Value.dammArray[currentDammIndex.Value].progress < 1)
        {
            dammArray.Value.dammArray[currentDammIndex.Value].progress += Time.deltaTime * dammBuildProgress.Value;
            return TaskStatus.Running;
        }
        EventSystem<Vector3, TileID>.RaiseEvent(EventType.CHANGE_TILE, targetPosition.Value, TileID.DAMM);
        dammArray.Value.dammArray[currentDammIndex.Value].buildDamm = true;
        // dammArray.Value.dammArray[currentDammIndex.Value].progress = 0;
        // dammArray.Value.dammArray[currentDammIndex.Value].amountBeavorsWorking = 0;
        return TaskStatus.Success;
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

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
    public SharedInt amountTilesDammFromWall;
    public SharedFloat dammBuildProgress;
    
    public override TaskStatus OnUpdate()
    {
        if (gridInfo.Value.tileIDs[currentDammIndex.Value] == TileID.WALL)
        {
            return TaskStatus.Failure;
        }
        
        if (dammArray.Value.dammArray[currentDammIndex.Value].progress < 1)
        {
            dammArray.Value.dammArray[currentDammIndex.Value].progress += Time.deltaTime * dammBuildProgress.Value;
            return TaskStatus.Running;
        }
        EventSystem<Vector3, TileID>.RaiseEvent(EventType.FORCE_CHANGE_TILE, targetPosition.Value, TileID.DAMM);
        dammArray.Value.dammArray[currentDammIndex.Value].buildDamm = true;
        amountTilesDammFromWall.Value = Mathf.Max(amountTilesDammFromWall.Value - 1, 2);
        return TaskStatus.Success;
    }
}

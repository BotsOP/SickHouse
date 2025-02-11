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
        if (GridManager.instance.CheckIfTileMatches(currentDammIndex.Value, EntityTileID.PAVEMENT))
        {
            return TaskStatus.Failure;
        }
        
        if (dammArray.Value.dammArray[currentDammIndex.Value].progress < 1)
        {
            dammArray.Value.dammArray[currentDammIndex.Value].progress += Time.deltaTime * dammBuildProgress.Value;
            return TaskStatus.Running;
        }

        dammArray.Value.dammArray[currentDammIndex.Value].progress = 1f;
        dammArray.Value.dammArray[currentDammIndex.Value].buildDamm = true;
        EventSystem<Vector3, EntityTileID>.RaiseEvent(EventType.FORCE_CHANGE_TILE, targetPosition.Value, EntityTileID.DAMM);
        amountTilesDammFromWall.Value = Mathf.Max(amountTilesDammFromWall.Value - 1, 2);
        return TaskStatus.Success;
    }
}

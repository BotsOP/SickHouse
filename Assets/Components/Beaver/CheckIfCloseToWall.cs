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
        Vector2Int posIndex = GridHelper.IndexToIndexPos(currentDammIndex.Value);
        int index = GridHelper.IndexPosToIndex(posIndex + new Vector2Int(0, amountTilesTooCloseToWall.Value));
        if (GridHelper.CheckIfTileMatches(index, EntityTileID.PAVEMENT))
        {
            amountTilesDammFromWall.Value++;
            dammArray.Value.dammArray[currentDammIndex.Value].amountBeavorsWorking--;
            return TaskStatus.Failure;
        }
        return TaskStatus.Running;
    }
}
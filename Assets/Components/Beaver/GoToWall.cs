using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;


public class GoToWall : Conditional
{
    public GridInfo gridInfo;
    public SharedVector3 targetPosition;
    public DammArray dammArray;
    public SharedInt amountTilesDammFromWall;
    public SharedInt currentDammIndex;
    public override TaskStatus OnUpdate()
    {
        Vector2Int posIndex = GridHelper.WorldPosToIndexPos(transform.position);
        for (int i = gridInfo.Value.wallDistance - amountTilesDammFromWall.Value; i >= 0; i--)
        {
            Vector2Int localIndex = new Vector2Int(posIndex.x, i);
            int index = GridHelper.IndexPosToIndex(localIndex);
            
            if(index >= gridInfo.Value.gridWidth * gridInfo.Value.gridHeight)
                break;
            
            if (GridHelper.CheckIfTileMatches(index, EntityTileID.PAVEMENT))
            {
            }
            int flip = 1;
            int indexToCheck = 0;
            int amountFlipped = 0;
            for (int j = 0; j < 200; j++)
            {
                int dammIndex = localIndex.x + (flip * indexToCheck);
                if (dammIndex < 0 || dammIndex >= gridInfo.Value.gridWidth)
                {
                    flip *= -1;
                    continue;
                }
                
                int newIndex = GridHelper.IndexPosToIndex(new Vector2Int(dammIndex, localIndex.y));
                
                if(newIndex >= gridInfo.Value.gridWidth * gridInfo.Value.gridHeight)
                    continue;
                
                if (dammArray.Value.dammArray[newIndex].amountBeavorsWorking < 3 && !dammArray.Value.dammArray[newIndex].buildDamm && !GridHelper.CheckIfTileMatches(newIndex, EntityTileID.DAMM))
                {
                    dammArray.Value.dammArray[newIndex].amountBeavorsWorking++;
                    targetPosition.Value = GridHelper.GetPosition(new Vector2Int(dammIndex, localIndex.y));
                    currentDammIndex.Value = newIndex;
                    return TaskStatus.Success;
                }

                if (localIndex.x + (flip * indexToCheck * -1) >= 0 && localIndex.x + (flip * indexToCheck * -1) < 100)
                {
                    flip *= -1;
                }
                else
                {
                    indexToCheck++;
                    continue;
                }
                if (amountFlipped == 1)
                {
                    indexToCheck++;
                    amountFlipped = 0;
                    continue;
                }
                amountFlipped++;
            }
        }
        return TaskStatus.Running;
    }
}
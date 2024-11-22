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
        Vector2Int posIndex = GetTile(transform.position);
        for (int i = gridInfo.Value.wallDistance - amountTilesDammFromWall.Value; i >= 0; i--)
        {
            Vector2Int localIndex = new Vector2Int(posIndex.x, i);
            int index = IndexPosToIndex(localIndex);
            
            if(index >= gridInfo.Value.gridWidth * gridInfo.Value.gridHeight)
                break;
            
            if (gridInfo.Value.tileIDs[index] == TileID.WALL)
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
                
                int newIndex = IndexPosToIndex(new Vector2Int(dammIndex, localIndex.y));
                
                if(newIndex >= gridInfo.Value.gridWidth * gridInfo.Value.gridHeight)
                    continue;
                
                if (dammArray.Value.dammArray[newIndex].amountBeavorsWorking < 3 && !dammArray.Value.dammArray[newIndex].buildDamm && gridInfo.Value.tileIDs[newIndex] != TileID.DAMM_WATER && gridInfo.Value.tileIDs[newIndex] != TileID.DAMM)
                {
                    dammArray.Value.dammArray[newIndex].amountBeavorsWorking++;
                    targetPosition.Value = GetPosition(new Vector2Int(dammIndex, localIndex.y));
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
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;


public class GoToWall : Conditional
{
    public GridInfo gridInfo;
    public SharedVector3 targetPosition;
    public DammArray dammArray;
    public override TaskStatus OnUpdate()
    {
        Vector2Int posIndex = GetTile(transform.position);
        for (int i = 0; i < 99; i++)
        {
            Vector2Int localIndex = posIndex + new Vector2Int(0, i);
            int index = IndexPosToIndex(localIndex);
            if(index >= gridInfo.Value.gridWidth * gridInfo.Value.gridHeight)
                break;
            if (gridInfo.Value.tilesFlattened[index] == TileID.WALL)
            {
                int flip = 1;
                int indexToCheck = 0;
                int amountFlipped = 0;
                for (int j = 0; j < 100; j++)
                {
                    int newIndex = localIndex.x + (flip * indexToCheck);
                    if (newIndex < 0 || newIndex >= 100)
                    {
                        break;
                    }
                    if (dammArray.Value.dammArray[newIndex].amountBeavorsWorking < 3)
                    {
                        dammArray.Value.dammArray[newIndex].amountBeavorsWorking++;
                        targetPosition.Value = GetPosition(new Vector2Int(newIndex, localIndex.y));
                        return TaskStatus.Success;
                    }

                    indexToCheck++;
                    if (localIndex.x + (flip * indexToCheck * -1) >= 0 || localIndex.x + (flip * indexToCheck * -1) < 100)
                    {
                        flip *= -1;
                    }
                    else
                    {
                        indexToCheck++;
                        continue;
                    }
                    if (amountFlipped == 2)
                    {
                        indexToCheck++;
                        amountFlipped = 0;
                        continue;
                    }
                    amountFlipped++;
                }
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


[System.Serializable]
public class GridInfoClass
{
    public TileID[] tilesFlattened;
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
}
[System.Serializable]
public class DammArray : SharedVariable<DammArrayClass>
{
    public static implicit operator DammArray(DammArrayClass value) { return new DammArray { Value = value }; }
}

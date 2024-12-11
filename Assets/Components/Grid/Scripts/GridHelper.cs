using System;
using UnityEngine;

public static class GridHelper
{
    public static int gridWidth;
    public static int gridHeight;
    public static int tileSize;
    public static Vector2Int WorldPosToIndexPos(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt((worldPos.x / tileSize) + (gridWidth * tileSize / 2f)), Mathf.RoundToInt((worldPos.z / tileSize) + (gridHeight * tileSize / 2f)));
    }
    public static Vector2Int WorldPosToIndexPos(Matrix4x4 matrix)
    {
        return WorldPosToIndexPos(new Vector3(matrix.GetRow(0).w, 0, matrix.GetRow(2).w));
    }
    public static Vector3 GetPosition(Vector2Int index)
    {
        return new Vector3((index.x * tileSize) - (gridWidth / tileSize / 2f), 0, (index.y * tileSize) - (gridHeight / tileSize / 2f));
    }
    public static int IndexPosToIndex(Vector2Int index)
    {
        return index.x * gridWidth + index.y % gridHeight;
    }
    
    public static Vector2Int IndexToIndexPos(int index)
    {
        return new Vector2Int(index / gridWidth, index % gridHeight);
    }
    public static Matrix4x4 IndexToMatrix4x4(Vector2Int posIndex)
    {
        Vector3 position = new Vector3(posIndex.x * tileSize, 0.0f, posIndex.y * tileSize) - new Vector3(gridWidth * tileSize / 2f, 0, gridHeight * tileSize / 2f);
        Matrix4x4 matrix4X4 = Matrix4x4.Translate(position) * Matrix4x4.Scale(new Vector3(tileSize, tileSize, tileSize));
        return matrix4X4;
    }
    public static Matrix4x4 IndexToMatrix4x4(int index)
    {
        Vector2Int posIndex = IndexToIndexPos(index);
        Vector3 position = new Vector3(posIndex.x * tileSize, 0.0f, posIndex.y * tileSize) - new Vector3(gridWidth * tileSize / 2f, 0, gridHeight * tileSize / 2f);
        Matrix4x4 matrix4X4 = Matrix4x4.Translate(position) * Matrix4x4.Scale(new Vector3(tileSize, tileSize, tileSize));
        return matrix4X4;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class GridManager : MonoBehaviour
{
    [SerializeField] private GameObject tile;
    [SerializeField] private TileObject tileObject;
    [SerializeField] private float tileSize = 1;
    [SerializeField] private int gridWidth = 100;
    [SerializeField] private int gridHeight = 100;
    
    private TileID[,] tiles;
    private GridTile[,] gridTiles;

    private void Awake()
    {
        tiles = new TileID[gridWidth, gridHeight];
        gridTiles = new GridTile[gridWidth, gridHeight];
        Vector3 middleGrid = new Vector3(gridWidth / 2f, 1, gridHeight / 2f);

        for (int x = 0; x < gridWidth; x++)
        for (int y = 0; y < gridHeight; y++)
        {
            int index = x * gridWidth + y % gridHeight;
            int randomIndex = Random.Range(0, tileObject.tiles.Length);

            Vector3 pos = new Vector3(x * tileSize, 0.0f, y * tileSize) -
                          new Vector3(gridWidth * tileSize / 2f, 0, gridHeight * tileSize / 2f);

            GameObject temp = Instantiate(tile, pos, Quaternion.identity, transform);
            MeshFilter meshFilter = temp.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = temp.GetComponent<MeshRenderer>();
            meshFilter.sharedMesh = tileObject.tiles[randomIndex].mesh;
            meshRenderer.material = tileObject.tiles[randomIndex].material;
            gridTiles[x, y] = new GridTile(meshRenderer, meshFilter);

            tiles[x, y] = (TileID)randomIndex;
        }
    }

    public void ChangeTile(Vector3 position, TileID tileID)
    {
        Vector2Int posIndex = GetTile(position);
        TileID oldTile = tiles[posIndex.x, posIndex.y];
        
        if (oldTile == tileID)
        {
            return;
        }

        gridTiles[posIndex.x, posIndex.y].meshFilter.sharedMesh = tileObject.tiles[(int)tileID].mesh;
        gridTiles[posIndex.x, posIndex.y].meshRenderer.material = tileObject.tiles[(int)tileID].material;
        Debug.Log($"Changed tile to {tileID}");
        tiles[posIndex.x, posIndex.y] = tileID;
    }

    private Vector2Int GetTile(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt((worldPos.x / tileSize) + (gridWidth * tileSize / 2f)), Mathf.RoundToInt((worldPos.z / tileSize) + (gridHeight * tileSize / 2f)));
    }
    
    private struct GridTile
    {
        public MeshRenderer meshRenderer;
        public MeshFilter meshFilter;

        public GridTile(MeshRenderer meshRenderer, MeshFilter meshFilter)
        {
            this.meshRenderer = meshRenderer;
            this.meshFilter = meshFilter;
        }
    }

    #region Matrix
    public static float ConvertDegToRad(float degrees)
    {
        return ((float)Math.PI / (float) 180) * degrees;
    }
    
    public static Matrix4x4 GetTranslationMatrix(Vector3 position)
    {
        return new Matrix4x4(new Vector4(1, 0, 0, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(position.x, position.y, position.z, 1));
    }

    public static Matrix4x4 GetRotationMatrix(Vector3 anglesDeg)
    {
        anglesDeg = new Vector3(ConvertDegToRad(anglesDeg[0]), ConvertDegToRad(anglesDeg[1]), ConvertDegToRad(anglesDeg[2]));

        Matrix4x4 rotationX = new Matrix4x4(new Vector4(1, 0, 0, 0), 
            new Vector4(0, Mathf.Cos(anglesDeg[0]), Mathf.Sin(anglesDeg[0]), 0), 
            new Vector4(0, -Mathf.Sin(anglesDeg[0]), Mathf.Cos(anglesDeg[0]), 0),
            new Vector4(0, 0, 0, 1));

        Matrix4x4 rotationY = new Matrix4x4(new Vector4(Mathf.Cos(anglesDeg[1]), 0, -Mathf.Sin(anglesDeg[1]), 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(Mathf.Sin(anglesDeg[1]), 0, Mathf.Cos(anglesDeg[1]), 0),
            new Vector4(0, 0, 0, 1));

        Matrix4x4 rotationZ = new Matrix4x4(new Vector4(Mathf.Cos(anglesDeg[2]), Mathf.Sin(anglesDeg[2]), 0, 0),
            new Vector4(-Mathf.Sin(anglesDeg[2]), Mathf.Cos(anglesDeg[2]), 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(0, 0, 0, 1));

        return rotationX * rotationY * rotationZ;
    }

    public static Matrix4x4 GetScaleMatrix(Vector3 scale)
    {
        return new Matrix4x4(new Vector4(scale.x, 0, 0, 0),
            new Vector4(0, scale.y, 0, 0),
            new Vector4(0, 0, scale.z, 0),
            new Vector4(0, 0, 0, 1));
    }

    public static Matrix4x4 Get_TRS_Matrix(Vector3 position, Vector3 rotationAngles, Vector3 scale) 
    {
        return GetTranslationMatrix(position) * GetRotationMatrix(rotationAngles) * GetScaleMatrix(scale);
    }
    
    public static Matrix4x4 Get_TRS_Matrix(Vector3 position, Vector3 rotationAngles) 
    {
        return GetTranslationMatrix(position) * GetRotationMatrix(rotationAngles);
    }
    
    public static Matrix4x4 Get_TRS_Matrix(Vector3 position) 
    {
        return GetTranslationMatrix(position);
    }
    #endregion
}
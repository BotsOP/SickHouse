using System;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime;
using Managers;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using VInspector;
using EventType = Managers.EventType;
using Random = UnityEngine.Random;


public class GridManagerPainter : MonoBehaviour
{
    private const int AmountTileIDs = 8;
    private const int AmountEntitiesOnOneTile = 3;

    private readonly static int GridBuffer = Shader.PropertyToID("gridBuffer");
    private readonly static int GridWidth = Shader.PropertyToID("gridWidth");
    private readonly static int GridHeight = Shader.PropertyToID("gridHeight");
    private readonly static int TileSize = Shader.PropertyToID("tileSize");
    private readonly static int GridFloorBuffer = Shader.PropertyToID("_GridFloorBuffer");
    
    [NonSerialized] public GridTileStruct[,] tileIDs;
    [NonSerialized] public List<List<Matrix4x4>> matricesList;
    
    [Tab("Grid Settings")]
    [Header("Grid")]
    public int gridWidth = 100;
    public int gridHeight = 100;
    public float tileSize = 1;
    [SerializeField] private GridObject gridObject;
    [SerializeField] private Material instanceMat;
    
    [Header("Misc")]
    [SerializeField] private EntityTileID startFillEntityTileID;

    [Tab("Tile Settings")]
    [SerializeField] private TileWrapper dirtTile;
    [SerializeField] private TileWrapper grassTile;
    [SerializeField] private TileWrapper waterTile;
    [SerializeField] private TileWrapper pavementTile;
    
    [SerializeField] private TileWrapper treeTile;
    [SerializeField] private TileWrapper damTile;
    [SerializeField] private TileWrapper cliffTile;
    
    private TileWrapper[] tiles;
    
    private ComputeBuffer gridSelectionBuffer;
    private Vector4[] gridSelectionBufferArray;
    
    private RenderParams[] renderParamsArray;
    private EntityTileID beforeSelectionEntityTileID;

    private List<int> tileIDToMatrixIndex;
    private int cachedIndex;
    private GridTileStruct[] cachedEntityTileID;

    private void AddMatrix(GridTileStruct gridTileStruct, Matrix4x4 matrix)
    {
        matricesList[GetMatrixIndex(gridTileStruct)].Add(matrix);
    }
    private int GetMatrixIndex(GridTileStruct gridTileStruct)
    {
        return tileIDToMatrixIndex[(int)gridTileStruct.tileID] + gridTileStruct.version;
    }

    private void OnDisable()
    {
        gridSelectionBuffer?.Release();

        EventSystem<Vector3, EntityTileID>.Unsubscribe(EventType.FORCE_CHANGE_TILE, ChangeTile);
    }

    private void Awake()
    {
        tiles = new TileWrapper[AmountTileIDs];
        tileIDs = new GridTileStruct[gridWidth * gridHeight, AmountEntitiesOnOneTile];

        GridHelper.gridWidth = gridWidth;
        GridHelper.gridHeight = gridHeight;
        GridHelper.tileSize = tileSize;
        GridHelper.tiles = tiles;
        GridHelper.tileIDs = tileIDs;
        
        EventSystem<Vector3, EntityTileID>.Subscribe(EventType.FORCE_CHANGE_TILE, ChangeTile);
        
        gridSelectionBuffer = new ComputeBuffer(gridWidth * gridHeight, sizeof(float) * 4);
        gridSelectionBufferArray = new Vector4[gridWidth * gridHeight];
        tileIDToMatrixIndex = new List<int>();
        cachedEntityTileID = new GridTileStruct[AmountEntitiesOnOneTile];

        // Vector3 middleGrid = new Vector3(gridWidth / 2f, 1, gridHeight / 2f);
        // bounds = new Bounds(new Vector3(gridWidth / 2f, 0, gridHeight / 2f), middleGrid * 1000);
        
        tiles[(int)EntityTileID.TREE] = treeTile;
        tiles[(int)EntityTileID.DAMM] = damTile;
        tiles[(int)EntityTileID.CLIFF] = cliffTile;
        tiles[(int)EntityTileID.DIRT] = dirtTile;
        tiles[(int)EntityTileID.GRASS] = grassTile;
        tiles[(int)EntityTileID.WATER] = waterTile;
        tiles[(int)EntityTileID.PAVEMENT] = pavementTile;

        matricesList = new List<List<Matrix4x4>>(tiles.Length);
        int counter = 0;
        for (int i = 0; i < tiles.Length; i++)
        {
            tileIDToMatrixIndex.Add(counter);
            for (int j = 0; j < tiles[i].renderSettings.Length; j++)
            {
                counter++;
                matricesList.Add(new List<Matrix4x4>());
            }
        }

        if (gridObject is null)
        {
            Debug.LogError($"Grid Object is null!");
            
            Vector2Int cachedIndex = new Vector2Int(0, 0);
            for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
            for (int z = 0; z < AmountEntitiesOnOneTile; z++)
            {
                int indexPosToIndex = GridHelper.IndexPosToIndex(new Vector2Int(x, y));
                GridTileStruct tileIDStruct = new GridTileStruct(EntityTileID.EMPTY, 0);
                if (z == tiles[(int)startFillEntityTileID].order)
                {
                    tileIDStruct = GetRandomTileStruct(startFillEntityTileID);
                }
            
                cachedIndex.x = x;
                cachedIndex.y = y;
                Matrix4x4 matrix4X4 = GridHelper.IndexToMatrix4x4(cachedIndex);
                tileIDs[indexPosToIndex, z] = tileIDStruct;
            
                if(tileIDStruct.tileID == 0)
                    continue;
                AddMatrix(tileIDStruct, matrix4X4);
            }
        }
        else
        {
            gridObject.Load();
            
            Vector2Int cachedIndex = new Vector2Int(0, 0);
            for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
            for (int z = 0; z < AmountEntitiesOnOneTile; z++)
            {
                int indexPosToIndex = GridHelper.IndexPosToIndex(new Vector2Int(x, y));
                GridTileStruct tileIDStruct = gridObject.tiles[indexPosToIndex, z];
            
                cachedIndex.x = x;
                cachedIndex.y = y;
                Matrix4x4 matrix4X4 = GridHelper.IndexToMatrix4x4(cachedIndex);
                tileIDs[indexPosToIndex, z] = tileIDStruct;
            
                if(tileIDStruct.tileID == 0)
                    continue;
                AddMatrix(tileIDStruct, matrix4X4);
            }
        }
        
        
        // waterSpots = waterSpots.OrderBy(x => x.y).ToList();

        renderParamsArray = new RenderParams[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
        for (int j = 0; j < tiles[i].renderSettings.Length; j++)
        {
            bool copied = false;
            for (int k = 0; k < tiles.Length; k++)
            for (int l = 0; l < tiles[k].renderSettings.Length; l++)
            {
                if (tiles[i].renderSettings[j].material == tiles[k].renderSettings[l].material && i > k)
                {
                    renderParamsArray[i] = renderParamsArray[k];
                    copied = true;
                    break;
                }
            }
            
            if(copied)
                continue;

            if (tiles[i].renderSettings[j].material == null)
            {
                Debug.LogError($"Material at index {i} in TileSettings is not set");
            }
            
            RenderParams renderParams = new RenderParams(tiles[i].renderSettings[j].material)
            {
                matProps = new MaterialPropertyBlock(),
            };
            renderParamsArray[i] = renderParams;
        }

        Array.Fill(gridSelectionBufferArray, Vector4.one);
        gridSelectionBuffer.SetData(gridSelectionBufferArray);
        instanceMat.SetBuffer(GridBuffer, gridSelectionBuffer);
        
        instanceMat.SetFloat(GridWidth, gridWidth);
        instanceMat.SetFloat(GridHeight, gridHeight);
        instanceMat.SetFloat(TileSize, tileSize);
    }

    private GridTileStruct GetRandomTileStruct(EntityTileID tileID)
    {
        if (tileID == EntityTileID.EMPTY)
            return new GridTileStruct(EntityTileID.EMPTY, 0);
        return new GridTileStruct(tileID, Random.Range(0, tiles[(int)tileID].renderSettings.Length));
    }
    private void ChangeTile(Vector3 position, EntityTileID entityTileID)
    {
        int index = GridHelper.IndexPosToIndex(GridHelper.WorldPosToIndexPos(position));
        GridTileStruct newTile = GetRandomTileStruct(entityTileID);
        
        if (entityTileID != EntityTileID.EMPTY)
        {
            int order = tiles[(int)entityTileID].order;
        
            GridTileStruct oldEntityTile = tileIDs[index, order];
            Matrix4x4 matrix4X4 = GridHelper.IndexToMatrix4x4(index);
        
            tileIDs[index, order] = newTile;
            matricesList[GetMatrixIndex(oldEntityTile)].RemoveSwapBack(matrix4X4);
            matricesList[GetMatrixIndex(newTile)].Add(matrix4X4);
            return;
        }

        for (int i = 0; i < AmountEntitiesOnOneTile; i++)
        {
            GridTileStruct oldEntityTile = tileIDs[index, i];
            Matrix4x4 matrix4X4 = GridHelper.IndexToMatrix4x4(index);
        
            tileIDs[index, i] = newTile;
            matricesList[GetMatrixIndex(oldEntityTile)].RemoveSwapBack(matrix4X4);
        }
    }
    private void Update()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if(i == matricesList.Count)
                break;
            if (matricesList[i].Count == 0)
                continue;

            for (int j = 0; j < tiles[i].renderSettings.Length; j++)
            {
                foreach (TextureWtihReference textureWtihReference in tiles[i].renderSettings[j].textures)
                {
                    renderParamsArray[i].matProps.SetTexture(textureWtihReference.textureName, textureWtihReference.texture);
                }
            
                Graphics.RenderMeshInstanced(renderParamsArray[i], tiles[i].renderSettings[j].mesh, 0, matricesList[GetMatrixIndex(new GridTileStruct((EntityTileID)i, j))]);
            }
        }
    }
}

















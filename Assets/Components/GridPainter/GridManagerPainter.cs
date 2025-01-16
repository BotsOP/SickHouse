using System;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime;
using Managers;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
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
    
    [Header("Grid")]
    public int gridWidth = 50;
    public int gridHeight = 50;
    public float tileSize = 1;
    [SerializeField] private TextAsset json;
    
    [Header("Misc")]
    [SerializeField] private EntityTileID startFillEntityTileID;

    [SerializeField] private TileWrapper emptyTile;
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
        if(gameObject.name == "Dummy object for fetching default variable values for vInspector's resettable variables feature")
            return;
        
        tiles = new TileWrapper[AmountTileIDs];
        tileIDs = new GridTileStruct[gridWidth * gridHeight, AmountEntitiesOnOneTile];
        
        EventSystem<Vector3, EntityTileID>.Subscribe(EventType.FORCE_CHANGE_TILE, ChangeTile);
        
        gridSelectionBuffer = new ComputeBuffer(gridWidth * gridHeight, sizeof(float) * 4);
        gridSelectionBufferArray = new Vector4[gridWidth * gridHeight];
        tileIDToMatrixIndex = new List<int>();
        cachedEntityTileID = new GridTileStruct[AmountEntitiesOnOneTile];
        
        Array.Fill(gridSelectionBufferArray, Vector4.one);
        gridSelectionBuffer.SetData(gridSelectionBufferArray);

        tiles[(int)EntityTileID.TREE] = treeTile;
        tiles[(int)EntityTileID.DAMM] = damTile;
        tiles[(int)EntityTileID.CLIFF] = cliffTile;
        tiles[(int)EntityTileID.DIRT] = dirtTile;
        tiles[(int)EntityTileID.GRASS] = grassTile;
        tiles[(int)EntityTileID.WATER] = waterTile;
        tiles[(int)EntityTileID.PAVEMENT] = pavementTile;
        tiles[(int)EntityTileID.EMPTY] = emptyTile;

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

        if (json is null)
        {
            Debug.LogError($"Grid Object is null!");
            return;
        }
        

        if (json is null)
        {
            Debug.LogError($"Grid Object is null!");
            
            Vector2Int cachedIndex = new Vector2Int(0, 0);
            for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
            for (int z = 0; z < AmountEntitiesOnOneTile; z++)
            {
                int indexPosToIndex = IndexPosToIndex(new Vector2Int(x, y));
                GridTileStruct tileIDStruct = new GridTileStruct(EntityTileID.EMPTY, 0);
                if (z == tiles[(int)startFillEntityTileID].order)
                {
                    tileIDStruct = GetRandomTileStruct(startFillEntityTileID);
                }
            
                cachedIndex.x = x;
                cachedIndex.y = y;
                Matrix4x4 matrix4X4 = IndexToMatrix4x4(cachedIndex);
                tileIDs[indexPosToIndex, z] = tileIDStruct;
            
                if(tileIDStruct.tileID == 0)
                    continue;
                AddMatrix(tileIDStruct, matrix4X4);
            }
        }
        else
        {
            tileIDs = GridObject.Load(json);
            
            Vector2Int cachedIndex = new Vector2Int(0, 0);
            for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
            for (int z = 0; z < AmountEntitiesOnOneTile; z++)
            {
                int indexPosToIndex = IndexPosToIndex(new Vector2Int(x, y));
                GridTileStruct tileIDStruct = tileIDs[indexPosToIndex, z];
                
                cachedIndex.x = x;
                cachedIndex.y = y;
                Matrix4x4 matrix4X4 = IndexToMatrix4x4(cachedIndex);
                tileIDs[indexPosToIndex, z] = tileIDStruct;
                
                if(tileIDStruct.tileID == EntityTileID.EMPTY)
                    continue;
                
                AddMatrix(tileIDStruct, matrix4X4);
            }

            Array.Fill(gridSelectionBufferArray, Vector4.one);
            gridSelectionBuffer.SetData(gridSelectionBufferArray);
            
            renderParamsArray = new RenderParams[tiles.Length];
            for (int i = 0; i < tiles.Length; i++)
            for (int j = 0; j < tiles[i].renderSettings.Length; j++)
            {
                bool copied = false;
                for (int k = 0; k < tiles.Length; k++)
                for (int l = 0; l < tiles[k].renderSettings.Length; l++)
                {
                    tiles[k].renderSettings[l].material.SetBuffer(GridBuffer, gridSelectionBuffer);
            
                    tiles[k].renderSettings[l].material.SetFloat(GridWidth, gridWidth);
                    tiles[k].renderSettings[l].material.SetFloat(GridHeight, gridHeight);
                    tiles[k].renderSettings[l].material.SetFloat(TileSize, tileSize);
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
                    shadowCastingMode = ShadowCastingMode.On,
                };
                renderParamsArray[i] = renderParams;
            }
        }

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
    }

    private GridTileStruct GetRandomTileStruct(EntityTileID tileID)
    {
        if (tileID == EntityTileID.EMPTY)
            return new GridTileStruct(EntityTileID.EMPTY, 0);
        return new GridTileStruct(tileID, Random.Range(0, tiles[(int)tileID].renderSettings.Length));
    }
    private void ChangeTile(Vector3 position, EntityTileID entityTileID)
    {
        int index = IndexPosToIndex(WorldPosToIndexPos(position));
        GridTileStruct newTile = GetRandomTileStruct(entityTileID);
        
        if (entityTileID != EntityTileID.EMPTY)
        {
            int order = tiles[(int)entityTileID].order;
        
            GridTileStruct oldEntityTile = tileIDs[index, order];
            Matrix4x4 matrix4X4 = IndexToMatrix4x4(index);
        
            tileIDs[index, order] = newTile;
            matricesList[GetMatrixIndex(oldEntityTile)].RemoveSwapBack(matrix4X4);
            matricesList[GetMatrixIndex(newTile)].Add(matrix4X4);
            return;
        }

        for (int i = 0; i < AmountEntitiesOnOneTile; i++)
        {
            GridTileStruct oldEntityTile = tileIDs[index, i];
            Matrix4x4 matrix4X4 = IndexToMatrix4x4(index);
        
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

            for (int j = 0; j < tiles[i].renderSettings.Length; j++)
            {
                foreach (TextureWtihReference textureWtihReference in tiles[i].renderSettings[j].textures)
                {
                    renderParamsArray[i].matProps.SetTexture(textureWtihReference.textureName, textureWtihReference.texture);
                }

                int index = GetMatrixIndex(new GridTileStruct((EntityTileID)i, j));
                if (matricesList[index].Count == 0)
                    continue;
                
                Graphics.RenderMeshInstanced(renderParamsArray[i], tiles[i].renderSettings[j].mesh, 0, matricesList[index]);
            }
        }
    }
    
    private void ChangeTile(int index, GridTileStruct[] entityTileID)
    {
        for (int i = 0; i < AmountEntitiesOnOneTile; i++)
        {
            GridTileStruct oldEntityTile = tileIDs[index, i];
            Matrix4x4 matrix4X4 = IndexToMatrix4x4(index);
        
            tileIDs[index, i] = entityTileID[i];
            matricesList[GetMatrixIndex(oldEntityTile)].RemoveSwapBack(matrix4X4);
            matricesList[GetMatrixIndex(entityTileID[i])].Add(matrix4X4);
        }
    }
    private void ChangeTile(Vector3 position, GridTileStruct entityTileID, int order)
    {
        int index = IndexPosToIndex(WorldPosToIndexPos(position));
        ChangeTile(index, entityTileID, order);
    }
    private void ChangeTile(Vector2Int indexPos, GridTileStruct entityTileID, int order)
    {
        int index = IndexPosToIndex(indexPos);
        ChangeTile(index, entityTileID, order);
    }
    private void ChangeTile(int index, GridTileStruct entityTileID, int order)
    {
        GridTileStruct oldEntityTile = tileIDs[index, order];
        Matrix4x4 matrix4X4 = IndexToMatrix4x4(index);
        
        tileIDs[index, order] = entityTileID;
        matricesList[GetMatrixIndex(oldEntityTile)].RemoveSwapBack(matrix4X4);
        matricesList[GetMatrixIndex(entityTileID)].Add(matrix4X4);

    }
    // private void ChangeTile(Vector3 position, EntityTileID entityTileID)
    // {
    //     int index = IndexPosToIndex(WorldPosToIndexPos(position));
    //     GridTileStruct newTile = GetRandomTileStruct(entityTileID);
    //     int order = tiles[(int)entityTileID].order;
    //     GridTileStruct oldEntityTile = tileIDs[index, order];
    //     Matrix4x4 matrix4X4 = IndexToMatrix4x4(index);
    //
    //     tileIDs[index, order] = newTile;
    //     matricesList[GetMatrixIndex(oldEntityTile)].RemoveSwapBack(matrix4X4);
    //     matricesList[GetMatrixIndex(newTile)].Add(matrix4X4);
    // }
    private void ChangeTile(Vector3 position, EntityTileID[] entityTileID)
    {
        for (int i = 0; i < entityTileID.Length; i++)
        {
            ChangeTile(position, entityTileID[i]);
        }
    }
    private void ChangeTile(int index, EntityTileID entityTileID)
    {
        GridTileStruct newTile = GetRandomTileStruct(entityTileID);
        int order = tiles[(int)entityTileID].order;
        GridTileStruct oldEntityTile = tileIDs[index, order];
        Matrix4x4 matrix4X4 = IndexToMatrix4x4(index);
        
        tileIDs[index, order] = newTile;
        matricesList[GetMatrixIndex(oldEntityTile)].RemoveSwapBack(matrix4X4);
        matricesList[GetMatrixIndex(newTile)].Add(matrix4X4);
    }
    
    public Vector2Int WorldPosToIndexPos(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt(((worldPos.x - tileSize / 2) / tileSize) + (gridWidth * tileSize / 2f)), Mathf.RoundToInt((worldPos.z / tileSize) + (gridHeight * tileSize / 2f)));
    }
    public Vector2Int WorldPosToIndexPos(Matrix4x4 matrix)
    {
        return WorldPosToIndexPos(new Vector3(matrix.GetRow(0).w, 0, matrix.GetRow(2).w));
    }
    public Vector3 GetPosition(Vector2Int index)
    {
        return new Vector3((index.x * tileSize) - (gridWidth / tileSize / 2f) + tileSize / 2, 0, (index.y * tileSize) - (gridHeight / tileSize / 2f));
    }
    public int IndexPosToIndex(Vector2Int index)
    {
        int indexPosToIndex = index.x * gridWidth + index.y % gridHeight;
        indexPosToIndex = math.clamp(indexPosToIndex, 0, gridHeight * gridWidth - 1);
        return indexPosToIndex;
    }
    public Vector2Int IndexToIndexPos(int index)
    {
        return new Vector2Int(index / gridWidth, index % gridHeight);
    }
    public Vector3 IndexToPos(int index)
    {
        return GetPosition(new Vector2Int(index / gridWidth, index % gridHeight));
    }
    public Matrix4x4 IndexToMatrix4x4(Vector2Int posIndex)
    {
        Vector3 position = new Vector3(posIndex.x * tileSize + tileSize / 2, 0.0f, posIndex.y * tileSize) - new Vector3(gridWidth * tileSize / 2f, 0, gridHeight * tileSize / 2f);
        Matrix4x4 matrix4X4 = Matrix4x4.Translate(position) * Matrix4x4.Scale(new Vector3(tileSize, tileSize, tileSize));
        return matrix4X4;
    }
    public Matrix4x4 IndexToMatrix4x4(int index)
    {
        Vector2Int posIndex = IndexToIndexPos(index);
        Vector3 position = new Vector3(posIndex.x * tileSize + tileSize / 2, 0.0f, posIndex.y * tileSize) - new Vector3(gridWidth * tileSize / 2f, 0, gridHeight * tileSize / 2f);
        Matrix4x4 matrix4X4 = Matrix4x4.Translate(position) * Matrix4x4.Scale(new Vector3(tileSize, tileSize, tileSize));
        return matrix4X4;
    }
    public bool CheckIfTileMatches(int index, EntityTileID tileID)
    {
        return tileIDs[index, tiles[(int)tileID].order].tileID == tileID;
    }
}

















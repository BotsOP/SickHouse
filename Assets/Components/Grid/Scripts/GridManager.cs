using System;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime;
using Managers;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;
using VInspector;
using EventType = Managers.EventType;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    private const int AmountTileIDs = 8;
    private const int AmountEntitiesOnOneTile = 3;

    private readonly static int GridBuffer = Shader.PropertyToID("gridBuffer");
    private readonly static int GridWidth = Shader.PropertyToID("gridWidth");
    private readonly static int GridHeight = Shader.PropertyToID("gridHeight");
    private readonly static int TileSize = Shader.PropertyToID("tileSize");
    private readonly static int SelectionColor = Shader.PropertyToID("_SelectionColor");
    private readonly static int AlbedoMap = Shader.PropertyToID("_AlbedoMap");
    private readonly static int GridFloorBuffer = Shader.PropertyToID("_GridFloorBuffer");

    [NonSerialized] public int wallDistance;
    [NonSerialized] public List<Vector2Int> waterSpots;
    [NonSerialized] public GridTileStruct[,] tileIDs;
    [NonSerialized] public List<List<Matrix4x4>> matricesList;
    
    [Tab("Grid Settings")]
    [Foldout("Grid Settings")]
    [Header("Grid")]
    public int gridWidth = 100;
    public int gridHeight = 100;
    public float tileSize = 1;
    public List<GameObject> racoons = new List<GameObject>();
    public List<GameObject> beavers = new List<GameObject>();
    [SerializeField] private GridObject gridObject;
    [SerializeField] private int amountApples = 100;

    [Header("Selection")]
    [SerializeField] private Color constrainedColor = Color.red;
    [SerializeField] private Color placeableColor = Color.green;
    [SerializeField] private Color requirementColor = Color.blue;
    
    [Header("Misc")]
    [SerializeField] private EntityTileID startFillEntityTileID;

    [Foldout("AppleTree")]
    [SerializeField] private int amountApplesPerCycle = 1;
    [SerializeField] private float appleCycleInSeconds = 1;
    [SerializeField] private int appleCost = 5;
    [SerializeField] private int maxAmountApplesProduced = 10;
    [SerializeField] private Apple applePrefab;

    [Foldout("Wall")]
    [SerializeField] private float wallCycleInSeconds = 10f;
    [SerializeField] private List<GameObject> wallPrefabs;
    [SerializeField] private GameObject bulldozerPrefab;
    [SerializeField] private VisualEffect bulldozerEffect;
    
    [Foldout("Creatures")]
    [SerializeField] private GameObject racoonPrefab;
    [SerializeField] private Transform racoonSpawnPoint;
    [SerializeField] private int racoonSpawnCost = 15;
    [SerializeField] private GameObject beavorPrefab;
    [SerializeField] private GameObject beavorGhostPrefab;
    [SerializeField] private Transform beavorSpawnPoint;
    [SerializeField] private int beavorSpawnCost = 15;

    [Foldout("Damm")]
    [SerializeField] private float dammSlowDown = 1;
    [SerializeField] private int checkAmountTilesInfrontOfWall = 3;
    [SerializeField] private VisualEffect dammVFX;

    [Tab("Tile Settings")]
    [SerializeField] private TileWrapper dirtTile;
    [SerializeField] private TileWrapper grassTile;
    [SerializeField] private TileWrapper waterTile;
    [SerializeField] private TileWrapper pavementTile;
    
    [SerializeField] private TileWrapper treeTile;
    [SerializeField] private TileWrapper damTile;
    [SerializeField] private TileWrapper cliffTile;
    [SerializeField] private TileWrapper emptyTile;
    
    private TileWrapper[] tiles;
    
    private ComputeBuffer gridSelectionBuffer;
    private Vector4[] gridSelectionBufferArray;
    private GraphicsBuffer dammVFXBuffer;
    private List<Vector3> dammVFXPositions;
    
    private RenderParams[] renderParamsArray;
    private EntityTileID beforeSelectionEntityTileID;

    private float lastTimeAppleCycle;
    private float lastTimeWallCycle;

    private float bulldozerProgress = 0;
    private int amountDamsAgainstWall = 0;
    private int previousSkyscraperIndex = 0;
    private Damm[] damms;
    private int[] appleTrees;

    private List<int> tileIDToMatrixIndex;
    private int cachedIndex = -1;
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
        dammVFXBuffer?.Release();
        
        EventSystem.Unsubscribe(EventType.SELECT_TILE_DOWN, StartChangingTile);
        EventSystem<Vector3, EntityTileID>.Unsubscribe(EventType.SELECT_TILE, PlacementSelection);
        EventSystem<Vector3, EntityTileID>.Unsubscribe(EventType.CHANGE_TILE, TryChangeTile);
        EventSystem<Vector3, EntityTileID[]>.Unsubscribe(EventType.CHANGE_TILE, TryChangeTile);
        EventSystem<Vector3, EntityTileID>.Unsubscribe(EventType.FORCE_CHANGE_TILE, ChangeTile);
        EventSystem.Unsubscribe(EventType.SPAWN_RACOON, SpawnRacoon);
        EventSystem.Unsubscribe(EventType.SPAWN_BEAVOR, SpawnBeavor);
        EventSystem<int, Vector3>.Unsubscribe(EventType.GAIN_APPLES, GainApples);
        EventSystem<int>.Unsubscribe(EventType.COLLECTED_APPLE, collectedAppleFromTree);
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
        
        EventSystem.Subscribe(EventType.SELECT_TILE_DOWN, StartChangingTile);
        EventSystem<Vector3, EntityTileID>.Subscribe(EventType.SELECT_TILE, PlacementSelection);
        EventSystem<Vector3, EntityTileID>.Subscribe(EventType.CHANGE_TILE, TryChangeTile);
        EventSystem<Vector3, EntityTileID[]>.Subscribe(EventType.CHANGE_TILE, TryChangeTile);
        EventSystem<Vector3, EntityTileID>.Subscribe(EventType.FORCE_CHANGE_TILE, ChangeTile);
        EventSystem.Subscribe(EventType.SPAWN_RACOON, SpawnRacoon);
        EventSystem.Subscribe(EventType.SPAWN_BEAVOR, SpawnBeavor);
        EventSystem<int, Vector3>.Subscribe(EventType.GAIN_APPLES, GainApples);
        EventSystem<int>.Subscribe(EventType.COLLECTED_APPLE, collectedAppleFromTree);
        
        gridSelectionBuffer = new ComputeBuffer(gridWidth * gridHeight, sizeof(float) * 4);
        gridSelectionBufferArray = new Vector4[gridWidth * gridHeight];
        appleTrees = new int[gridWidth * gridHeight];
        dammVFXPositions = new List<Vector3>(gridWidth);
        dammVFXBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, gridWidth, sizeof(float) * 3);
        dammVFX.SetGraphicsBuffer("OffsetPositions", dammVFXBuffer);
        tileIDToMatrixIndex = new List<int>();
        cachedEntityTileID = new GridTileStruct[AmountEntitiesOnOneTile];
        waterSpots = new List<Vector2Int>();

        wallDistance = gridHeight;
        
        // Vector3 middleGrid = new Vector3(gridWidth / 2f, 1, gridHeight / 2f);
        // bounds = new Bounds(new Vector3(gridWidth / 2f, 0, gridHeight / 2f), middleGrid * 1000);
        
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

        if (gridObject is null)
        {
            Debug.LogError($"Grid Object is null!");
            return;
        }
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
            
            if(tileIDStruct.tileID == EntityTileID.EMPTY)
                continue;
            if (tileIDStruct.tileID == EntityTileID.WATER)
                waterSpots.Add(new Vector2Int(x, y));
            AddMatrix(tileIDStruct, matrix4X4);
        }
        waterSpots = waterSpots.OrderBy(x => x.y).ToList();

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
            };
            renderParamsArray[i] = renderParams;
        }
        
        GridInfo gridInfo = new GridInfo
        {
            Value = this,
        };
        GlobalVariables.Instance.SetVariable("gridInfo", gridInfo);

        damms = new Damm[gridWidth * gridHeight];
        for (int i = 0; i < gridWidth * gridHeight; i++)
        {
            damms[i] = new Damm();
        }
        DammArrayClass dammArrayClass = new DammArrayClass
        {
            dammArray = damms,
        };
        DammArray dammArray = new DammArray
        {
            Value = dammArrayClass,
        };
        GlobalVariables.Instance.SetVariable("DammArray", dammArray);
    }
    
    
    private void CacheTile(int index)
    {
        cachedIndex = index;
        for (int i = 0; i < AmountEntitiesOnOneTile; i++)
        {
            cachedEntityTileID[i] = tileIDs[index, i];
        }
    }

    private GridTileStruct GetRandomTileStruct(EntityTileID tileID)
    {
        return new GridTileStruct(tileID, Random.Range(0, tiles[(int)tileID].renderSettings.Length));
    }

    private void StartChangingTile()
    {
    }

    private void PlacementSelection(Vector3 position, EntityTileID entityTileID)
    {
        Vector2Int posIndex = GridHelper.WorldPosToIndexPos(position);
        if(posIndex.x < 0 || posIndex.x >= gridWidth || posIndex.y < 0 || posIndex.y >= gridHeight)
            return;

        if (cachedIndex != -1)
        {
            ChangeTile(cachedIndex, cachedEntityTileID);
        }

        int index = GridHelper.IndexPosToIndex(posIndex);
        CacheTile(index);

        void AreaSelection(int localIndex)
        {
            gridSelectionBufferArray[localIndex] = placeableColor;
        }
        GetAreaSelection(entityTileID, posIndex, AreaSelection);

        int amountMatchingRequiredTiles = 0;
        void RequiredSelection(int localIndex) 
        {
            gridSelectionBufferArray[localIndex] = requirementColor;
            amountMatchingRequiredTiles++;
        }
        GetRequiredSelection(entityTileID, posIndex, RequiredSelection);

        void ConstraintSelection(int localIndex) 
        { 
            gridSelectionBufferArray[localIndex] = constrainedColor;
            gridSelectionBufferArray[index] = constrainedColor;
        }
        GetConstraintSelection(entityTileID, posIndex, ConstraintSelection);

        if (entityTileID == EntityTileID.TREE)
        {
            int amountRequiredTiles = tiles[(int)EntityTileID.TREE].TileGameSettings.placementRequirements[0].amountRequiredTiles;
            float lockedX = position.x > 0 ? (int)(position.x + tileSize / 2) : (int)(position.x - tileSize / 2);
            float lockedZ = position.z > 0 ? (int)(position.z + tileSize / 2) : (int)(position.z - tileSize / 2);
            Vector3 lockedPosition = new Vector3(lockedX, 0, lockedZ);
            EventSystem<int, int, Color, Vector3>.RaiseEvent(EventType.UPDATE_SELECTION_TEXT, 
                                                             amountMatchingRequiredTiles, 
                                                             amountRequiredTiles, 
                                                             amountMatchingRequiredTiles>=amountRequiredTiles?placeableColor:requirementColor, 
                                                             lockedPosition);
        }
        
        ChangeTile(index,  GetRandomTileStruct(entityTileID), tiles[(int)entityTileID].order);
        
        gridSelectionBuffer.SetData(gridSelectionBufferArray);
        Array.Fill(gridSelectionBufferArray, Vector4.one);
    }
    
    private void TryChangeTile(Vector3 position, EntityTileID entityTileID)
    {
        Array.Fill(gridSelectionBufferArray, Vector4.one);
        gridSelectionBuffer.SetData(gridSelectionBufferArray);
        
        Vector2Int posIndex = GridHelper.WorldPosToIndexPos(position);
        if (posIndex.x < 0 || posIndex.x >= gridWidth || posIndex.y < 0 || posIndex.y >= gridHeight)
        {
            cachedIndex = -1;
            return;
        }
        
        int order = tiles[(int)entityTileID].order;
        int index = GridHelper.IndexPosToIndex(posIndex);
        GridTileStruct oldGridTileStruct = cachedEntityTileID[order];
        
        int amountMatchingRequiredTiles = 0;
        bool requiredPlacement = true;
        void RequiredSelection(int localIndex) 
        {
            gridSelectionBufferArray[localIndex] = requirementColor;
            amountMatchingRequiredTiles++;
        }
        GetRequiredSelection(entityTileID, posIndex, RequiredSelection);
        if (tiles[(int)entityTileID].TileGameSettings.placementRequirements.Length > 0)
        {
            int amountRequiredTiles = tiles[(int)entityTileID].TileGameSettings.placementRequirements[0].amountRequiredTiles;
            requiredPlacement = amountMatchingRequiredTiles >= amountRequiredTiles;
        }
        bool constrained = GetConstraintSelection(entityTileID, posIndex);
        bool hasEnoughApples = amountApples >= tiles[(int)entityTileID].TileGameSettings.appleCost;

        if (oldGridTileStruct.tileID == entityTileID || !requiredPlacement || !constrained || !hasEnoughApples)
        {
            ChangeTile(cachedIndex, cachedEntityTileID);
            cachedIndex = -1;
            return;
        }
        cachedIndex = -1;

        if (entityTileID == EntityTileID.WATER)
        {            
            waterSpots.Add(GridHelper.IndexToIndexPos(index));
            waterSpots = waterSpots.OrderBy(x => x.y).ToList();
        }
        
        ChangeTile(index, GetRandomTileStruct(entityTileID), order);

        GainApples(-tiles[(int)entityTileID].TileGameSettings.appleCost, position);
        EventSystem<int>.RaiseEvent(EventType.AMOUNT_APPLES, amountApples);
    }
    
    private void TryChangeTile(Vector3 position, EntityTileID[] entityTileID)
    {
        Array.Fill(gridSelectionBufferArray, Vector4.one);
        gridSelectionBuffer.SetData(gridSelectionBufferArray);
        
        Vector2Int posIndex = GridHelper.WorldPosToIndexPos(position);
        if (posIndex.x < 0 || posIndex.x >= gridWidth || posIndex.y < 0 || posIndex.y >= gridHeight)
        {
            cachedIndex = -1;
            return;
        }

        for (int i = 0; i < entityTileID.Length; i++)
        {
            int index = GridHelper.IndexPosToIndex(posIndex);
            GridTileStruct oldGridTileStruct = cachedEntityTileID[i];
        
            int amountMatchingRequiredTiles = 0;
            bool requiredPlacement = true;
            void RequiredSelection(int localIndex) 
            {
                gridSelectionBufferArray[localIndex] = requirementColor;
                amountMatchingRequiredTiles++;
            }
            GetRequiredSelection(entityTileID[i], posIndex, RequiredSelection);
            if (tiles[(int)entityTileID[i]].TileGameSettings.placementRequirements.Length > 0)
            {
                int amountRequiredTiles = tiles[(int)entityTileID[i]].TileGameSettings.placementRequirements[0].amountRequiredTiles;
                requiredPlacement = amountMatchingRequiredTiles >= amountRequiredTiles;
            }
            bool constrained = GetConstraintSelection(entityTileID[i], posIndex);
            bool hasEnoughApples = amountApples >= tiles[(int)entityTileID[i]].TileGameSettings.appleCost;

            if (oldGridTileStruct.tileID == entityTileID[i] || !requiredPlacement || !constrained || !hasEnoughApples)
            {
                ChangeTile(cachedIndex, cachedEntityTileID[i].tileID);
                continue;
            }

            if (entityTileID[i] == EntityTileID.WATER)
            {            
                waterSpots.Add(GridHelper.IndexToIndexPos(index));
                waterSpots = waterSpots.OrderBy(x => x.y).ToList();
            }
        
            ChangeTile(index, GetRandomTileStruct(entityTileID[i]), i);

            GainApples(-tiles[(int)entityTileID[i]].TileGameSettings.appleCost, position);
            EventSystem<int>.RaiseEvent(EventType.AMOUNT_APPLES, amountApples);
        }
        cachedIndex = -1;
    }
    
    private void ChangeTile(int index, GridTileStruct[] entityTileID)
    {
        for (int i = 0; i < AmountEntitiesOnOneTile; i++)
        {
            GridTileStruct oldEntityTile = tileIDs[index, i];
            Matrix4x4 matrix4X4 = GridHelper.IndexToMatrix4x4(index);
        
            tileIDs[index, i] = entityTileID[i];
            matricesList[GetMatrixIndex(oldEntityTile)].RemoveSwapBack(matrix4X4);
            matricesList[GetMatrixIndex(entityTileID[i])].Add(matrix4X4);
        }
    }
    private void ChangeTile(Vector3 position, GridTileStruct entityTileID, int order)
    {
        int index = GridHelper.IndexPosToIndex(GridHelper.WorldPosToIndexPos(position));
        ChangeTile(index, entityTileID, order);
    }
    private void ChangeTile(Vector2Int indexPos, GridTileStruct entityTileID, int order)
    {
        int index = GridHelper.IndexPosToIndex(indexPos);
        ChangeTile(index, entityTileID, order);
    }
    private void ChangeTile(int index, GridTileStruct entityTileID, int order)
    {
        GridTileStruct oldEntityTile = tileIDs[index, order];
        Matrix4x4 matrix4X4 = GridHelper.IndexToMatrix4x4(index);
        
        tileIDs[index, order] = entityTileID;
        matricesList[GetMatrixIndex(oldEntityTile)].RemoveSwapBack(matrix4X4);
        matricesList[GetMatrixIndex(entityTileID)].Add(matrix4X4);
    }
    private void ChangeTile(Vector3 position, EntityTileID entityTileID)
    {
        int index = GridHelper.IndexPosToIndex(GridHelper.WorldPosToIndexPos(position));
        GridTileStruct newTile = GetRandomTileStruct(entityTileID);
        int order = tiles[(int)entityTileID].order;
        GridTileStruct oldEntityTile = tileIDs[index, order];
        Matrix4x4 matrix4X4 = GridHelper.IndexToMatrix4x4(index);
        
        tileIDs[index, order] = newTile;
        matricesList[GetMatrixIndex(oldEntityTile)].RemoveSwapBack(matrix4X4);
        matricesList[GetMatrixIndex(newTile)].Add(matrix4X4);
    }
    private void ChangeTile(int index, EntityTileID entityTileID)
    {
        GridTileStruct newTile = GetRandomTileStruct(entityTileID);
        int order = tiles[(int)entityTileID].order;
        GridTileStruct oldEntityTile = tileIDs[index, order];
        Matrix4x4 matrix4X4 = GridHelper.IndexToMatrix4x4(index);
        
        tileIDs[index, order] = newTile;
        matricesList[GetMatrixIndex(oldEntityTile)].RemoveSwapBack(matrix4X4);
        matricesList[GetMatrixIndex(newTile)].Add(matrix4X4);
    }
    
    private void SpawnRacoon()
    {
        if(amountApples < racoonSpawnCost)
            return;
        
        GainApples(-racoonSpawnCost, racoonSpawnPoint.position);
        racoons.Add(Instantiate(racoonPrefab, racoonSpawnPoint.position, racoonSpawnPoint.rotation));
        EventSystem<int>.RaiseEvent(EventType.AMOUNT_RACCOONS, racoons.Count + 1);
    }
    
    private void SpawnBeavor()
    {
        int amountDams = 99;
        // int amountDams = matricesList[(int)TileID.DAMM_WATER].Count;
        if(amountApples < beavorSpawnCost || ((beavers.Count + 1) > amountDams * 2))
            return;
        
        GainApples(-beavorSpawnCost, beavorSpawnPoint.position);
        beavers.Add(Instantiate(beavorPrefab, beavorSpawnPoint.position, beavorSpawnPoint.rotation));
        EventSystem<int>.RaiseEvent(EventType.AMOUNT_BEAVERS, beavers.Count + 1);
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

        UpdateApples();
        
        UpdateWall();

        int amountDams = 99;
        // int amountDams = matricesList[(int)TileID.DAMM_WATER].Count;
        if (beavers.Count > amountDams * 2 && beavers.Count > 1)
        {
            Destroy(Instantiate(beavorGhostPrefab, beavers[0].transform.position, Quaternion.identity), 0.99f);
            Destroy(beavers[0]);
            beavers.RemoveAtSwapBack(0);
            EventSystem<int>.RaiseEvent(EventType.AMOUNT_BEAVERS, beavers.Count + 1);
        }
        
        EventSystem<int>.RaiseEvent(EventType.AMOUNT_APPLES, amountApples);
    }
    private void UpdateApples()
    {
        if (Time.time > lastTimeAppleCycle + appleCycleInSeconds)
        {
            lastTimeAppleCycle = Time.time;
            for (int i = 0; i < matricesList[(int)EntityTileID.TREE].Count; i++)
            {
                Vector3 position = matricesList[(int)EntityTileID.TREE][i].GetPosition();
                int index = GridHelper.IndexPosToIndex(GridHelper.WorldPosToIndexPos(position));
                
                if(appleTrees[index] >= maxAmountApplesProduced || index == cachedIndex)
                    continue;
                
                for (int j = 0; j < amountApplesPerCycle; j++)
                {
                    if(appleTrees[index] >= maxAmountApplesProduced)
                        break;
                    
                    appleTrees[index]++;
                    Vector3 randomOffset = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
                    Instantiate(applePrefab, position + randomOffset, Quaternion.identity).treeIndex = index;
                }
            }
        }
    }

    private void UpdateWall()
    {
        bool hitDamm = false;
        for (int x = 0; x < gridWidth; x++)
        {
            Vector2Int posIndex = new Vector2Int(x, wallDistance - 1);
            int index = GridHelper.IndexPosToIndex(posIndex);
            if (GridHelper.CheckIfTileMatches(index, EntityTileID.DAMM) && damms[index].progress < 1.5f && damms[index].buildDamm)
            {
                amountDamsAgainstWall++;
                dammVFXPositions.Add(GridHelper.GetPosition(posIndex));
                damms[index].progress = 2;
                hitDamm = true;
                lastTimeWallCycle += dammSlowDown;
            }
        }

        if (hitDamm)
        {
            dammVFXBuffer.SetData(dammVFXPositions);
            dammVFX.SetGraphicsBuffer("OffsetPositions", dammVFXBuffer);
            dammVFX.SetInt("AmountDams", amountDamsAgainstWall);
            bulldozerEffect.SetFloat("AmountDams01", (float)amountDamsAgainstWall / gridWidth);
        }

        bulldozerProgress += Time.deltaTime / (wallCycleInSeconds + amountDamsAgainstWall * dammSlowDown);
        bulldozerPrefab.transform.position = new Vector3(0, 0, (2 - bulldozerProgress + wallDistance - gridHeight / 2) * tileSize);
        
        if (Time.time > lastTimeWallCycle + wallCycleInSeconds)
        {
            bulldozerProgress = 0;
            amountDamsAgainstWall = 0;
            dammVFXPositions.Clear();
            dammVFXBuffer.SetData(dammVFXPositions);
            dammVFX.SetGraphicsBuffer("OffsetPositions", dammVFXBuffer);
            dammVFX.SetInt("AmountDams", amountDamsAgainstWall);

            lastTimeWallCycle = Time.time;
            wallDistance--;

            if (wallDistance <= 0)
            {
                EventSystem.RaiseEvent(EventType.GAME_OVER);
                return;
            }
            
            if ((gridHeight - wallDistance) % 2 == 0 & (gridHeight - wallDistance) > 5)
            {
                int skyscraperIndex = wallDistance / 2 % wallPrefabs.Count;
                previousSkyscraperIndex = skyscraperIndex;
                Instantiate(wallPrefabs[(wallDistance / 2 + previousSkyscraperIndex) % wallPrefabs.Count], new Vector3(0, 0, wallDistance - (gridHeight * tileSize / 2f) + tileSize * 5), Quaternion.identity).transform.GetChild(0).GetComponent<Animation>().Play();
            }
            
            GridTileStruct[] cityGridTileStructs = new GridTileStruct[AmountEntitiesOnOneTile];
            cityGridTileStructs[tiles[(int)EntityTileID.PAVEMENT].order] = GetRandomTileStruct(EntityTileID.PAVEMENT);
            for (int x = 0; x < gridWidth; x++)
            {
                int dammIndex = GridHelper.IndexPosToIndex(new Vector2Int(x, wallDistance));
                if (damms[dammIndex].buildDamm && GridHelper.CheckIfTileMatches(dammIndex, EntityTileID.DAMM))
                {
                    damms[dammIndex].amountBeavorsWorking = 0;
                    damms[dammIndex].progress = 0;
                    damms[dammIndex].buildDamm = false;
                }
                
                ChangeTile(dammIndex, cityGridTileStructs);
            }
        }
    }
    
    private void GainApples(int amount, Vector3 worldPos)
    {
        amountApples += amount;
        EventSystem<int, Vector3>.RaiseEvent(EventType.CHANGE_AMOUNT_APPLES, amount, worldPos);
    }
    private void collectedAppleFromTree(int treeIndex)
    {
        appleTrees[treeIndex]--;
    }
    
    private void GetAreaSelection(EntityTileID entityTileID, Vector2Int posIndex, Action<int> callback)
    {
        Vector2Int cachedIndex = new Vector2Int();
        foreach (AreaSelection areaSelection in tiles[(int)entityTileID].TileGameSettings.selection)
        {
            foreach (SelectionBox placementConstraintSelectionBox in areaSelection.selectionBoxes)
            {
                Vector2Int lowerCorner = new Vector2Int(Mathf.CeilToInt(placementConstraintSelectionBox.position.x - placementConstraintSelectionBox.size.x / 2f), 
                                                        Mathf.CeilToInt(placementConstraintSelectionBox.position.y - placementConstraintSelectionBox.size.y / 2f));
                Vector2Int upperCorner = new Vector2Int(Mathf.CeilToInt(placementConstraintSelectionBox.position.x + placementConstraintSelectionBox.size.x / 2f), 
                                                        Mathf.CeilToInt(placementConstraintSelectionBox.position.y + placementConstraintSelectionBox.size.y / 2f));
                
                for (int x = lowerCorner.x; x < upperCorner.x; x++)
                for (int y = lowerCorner.y; y < upperCorner.y; y++)
                {
                    cachedIndex.x = Mathf.Clamp(posIndex.x + x, 0, gridWidth);
                    cachedIndex.y = Mathf.Clamp(posIndex.y + y, 0, gridHeight);
                    int index = cachedIndex.x * gridWidth + cachedIndex.y % gridHeight;
                    callback?.Invoke(index);
                }
            }
        }
    }
    
    private bool GetConstraintSelection(EntityTileID entityTileID, Vector2Int posIndex, Action<int> callback = null)
    {
        Vector2Int cachedIndex = new Vector2Int();
        bool meetsRequirements = true;
        foreach (AreaConstraint placementConstraint in tiles[(int)entityTileID].TileGameSettings.placementConstraints)
        {
            foreach (SelectionBox placementConstraintSelectionBox in placementConstraint.selectionBoxes)
            {
                Vector2Int lowerCorner = new Vector2Int(Mathf.CeilToInt(placementConstraintSelectionBox.position.x - placementConstraintSelectionBox.size.x / 2f), 
                                                        Mathf.CeilToInt(placementConstraintSelectionBox.position.y - placementConstraintSelectionBox.size.y / 2f));
                Vector2Int upperCorner = new Vector2Int(Mathf.CeilToInt(placementConstraintSelectionBox.position.x + placementConstraintSelectionBox.size.x / 2f), 
                                                        Mathf.CeilToInt(placementConstraintSelectionBox.position.y + placementConstraintSelectionBox.size.y / 2f));
                
                for (int x = lowerCorner.x; x < upperCorner.x; x++)
                for (int y = lowerCorner.y; y < upperCorner.y; y++)
                {
                    cachedIndex.x = Mathf.Clamp(posIndex.x + x, 0, gridWidth);
                    cachedIndex.y = Mathf.Clamp(posIndex.y + y, 0, gridHeight);
                    bool invoke = true;
                    for (int i = 0; i < AmountEntitiesOnOneTile; i++)
                    {
                        EntityTileID tileID = tileIDs[GridHelper.IndexPosToIndex(cachedIndex), i].tileID;
                        if (x == 0 && y == 0)
                        {
                            tileID = cachedEntityTileID[i].tileID;
                        }
                        foreach (EntityTileID t in placementConstraint.tileIDs)
                        {
                            if (tileID != t)
                                continue;

                            int index = cachedIndex.x * gridWidth + cachedIndex.y % gridHeight;
                            callback?.Invoke(index);
                            meetsRequirements = false;
                            break;
                        }   
                    }
                }
            }
        }
        return meetsRequirements;
    }

    private bool GetRequiredSelection(EntityTileID entityTileID, Vector2Int posIndex, Action<int> callback = null)
    {
        Vector2Int cachedIndex = new Vector2Int();
        bool meetsRequirements = true;
        foreach (AreaRequirement areaRequirement in tiles[(int)entityTileID].TileGameSettings.placementRequirements)
        {
            int requirementTileAmount = 0;
            foreach (SelectionBox placementConstraintSelectionBox in areaRequirement.selectionBoxes)
            {
                Vector2Int lowerCorner = new Vector2Int(Mathf.CeilToInt(placementConstraintSelectionBox.position.x - placementConstraintSelectionBox.size.x / 2f), 
                                                        Mathf.CeilToInt(placementConstraintSelectionBox.position.y - placementConstraintSelectionBox.size.y / 2f));
                Vector2Int upperCorner = new Vector2Int(Mathf.CeilToInt(placementConstraintSelectionBox.position.x + placementConstraintSelectionBox.size.x / 2f), 
                                                        Mathf.CeilToInt(placementConstraintSelectionBox.position.y + placementConstraintSelectionBox.size.y / 2f));
                
                for (int x = lowerCorner.x; x < upperCorner.x; x++)
                for (int y = lowerCorner.y; y < upperCorner.y; y++)
                {
                    cachedIndex.x = Mathf.Clamp(posIndex.x + x, 0, gridWidth);
                    cachedIndex.y = Mathf.Clamp(posIndex.y + y, 0, gridHeight);
                    for (int i = 0; i < AmountEntitiesOnOneTile; i++)
                    {
                        EntityTileID tileID = tileIDs[GridHelper.IndexPosToIndex(cachedIndex), i].tileID;
                        if (x == 0 && y == 0)
                        {
                            tileID = cachedEntityTileID[i].tileID;
                        }
                        foreach (EntityTileID t in areaRequirement.tileIDs)
                        {
                            if (tileID != t)
                                continue;

                            requirementTileAmount++;
                            int index = cachedIndex.x * gridWidth + cachedIndex.y % gridHeight;
                            callback?.Invoke(index);
                            break;
                        }
                    }
                }
            }
            if (requirementTileAmount < areaRequirement.amountRequiredTiles)
            {
                meetsRequirements = false;
            }
        }
        return meetsRequirements;
    }
}

















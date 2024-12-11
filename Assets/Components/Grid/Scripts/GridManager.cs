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
    private const int AmountTileIDs = 3;
    private const int AmountFloorTileIDs = 5;

    private readonly static int GridBuffer = Shader.PropertyToID("gridBuffer");
    private readonly static int GridWidth = Shader.PropertyToID("gridWidth");
    private readonly static int GridHeight = Shader.PropertyToID("gridHeight");
    private readonly static int TileSize = Shader.PropertyToID("tileSize");
    private readonly static int SelectionColor = Shader.PropertyToID("_SelectionColor");
    private readonly static int AlbedoMap = Shader.PropertyToID("_AlbedoMap");
    private readonly static int GridFloorBuffer = Shader.PropertyToID("_GridFloorBuffer");

    [NonSerialized] public int wallDistance;
    [NonSerialized] public List<Vector2Int> waterSpots;
    [NonSerialized] public EntityTileID[] tileIDs;
    [NonSerialized] public FloorTileID[] floorTileIDs;
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
    [SerializeField] private Material instanceMat;
    [SerializeField] private Material floorMat;
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
    [SerializeField] private FloorTileWrapper dirtTile;
    [SerializeField] private FloorTileWrapper grassTile;
    [SerializeField] private FloorTileWrapper waterTile;
    [SerializeField] private FloorTileWrapper pavementTile;
    
    [SerializeField] private TileWrapper treeTile;
    [SerializeField] private TileWrapper damTile;
    [SerializeField] private TileWrapper cliffTile;
    
    private TileWrapper[] tiles;
    private FloorTileWrapper[] floorTiles;
    
    private ComputeBuffer gridSelectionBuffer;
    private Vector4[] gridSelectionBufferArray;
    private ComputeBuffer gridFloorBuffer;
    private int[] gridFloorBufferArray;
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


    private void OnDisable()
    {
        gridSelectionBuffer?.Release();
        dammVFXBuffer?.Release();
        gridFloorBuffer?.Release();
        
        EventSystem.Unsubscribe(EventType.SELECT_TILE_DOWN, StartChangingTile);
        EventSystem<Vector3, EntityTileID>.Unsubscribe(EventType.SELECT_TILE, PlacementSelection);
        EventSystem<Vector3, EntityTileID>.Unsubscribe(EventType.CHANGE_TILE, TryChangeBothTile);
        EventSystem<Vector3, EntityTileID, FloorTileID>.Unsubscribe(EventType.FORCE_CHANGE_TILE, ChangeBothTile);
        EventSystem<Vector3, EntityTileID>.Unsubscribe(EventType.FORCE_CHANGE_ENTITY_TILE, ChangeEntityTile);
        EventSystem.Unsubscribe(EventType.SPAWN_RACOON, SpawnRacoon);
        EventSystem.Unsubscribe(EventType.SPAWN_BEAVOR, SpawnBeavor);
        EventSystem<int, Vector3>.Unsubscribe(EventType.GAIN_APPLES, GainApples);
        EventSystem<int>.Unsubscribe(EventType.COLLECTED_APPLE, collectedAppleFromTree);
    }

    private void Awake()
    {
        EventSystem.Subscribe(EventType.SELECT_TILE_DOWN, StartChangingTile);
        EventSystem<Vector3, EntityTileID>.Subscribe(EventType.SELECT_TILE, PlacementSelection);
        EventSystem<Vector3, EntityTileID>.Subscribe(EventType.CHANGE_TILE, TryChangeBothTile);
        EventSystem<Vector3, EntityTileID, FloorTileID>.Subscribe(EventType.FORCE_CHANGE_TILE, ChangeBothTile);
        EventSystem<Vector3, EntityTileID>.Subscribe(EventType.FORCE_CHANGE_ENTITY_TILE, ChangeEntityTile);
        EventSystem.Subscribe(EventType.SPAWN_RACOON, SpawnRacoon);
        EventSystem.Subscribe(EventType.SPAWN_BEAVOR, SpawnBeavor);
        EventSystem<int, Vector3>.Subscribe(EventType.GAIN_APPLES, GainApples);
        EventSystem<int>.Subscribe(EventType.COLLECTED_APPLE, collectedAppleFromTree);
        
        tileIDs = new EntityTileID[gridWidth * gridHeight];
        gridSelectionBuffer = new ComputeBuffer(gridWidth * gridHeight, sizeof(float) * 4);
        gridSelectionBufferArray = new Vector4[gridWidth * gridHeight];
        appleTrees = new int[gridWidth * gridHeight];
        dammVFXPositions = new List<Vector3>(gridWidth);
        dammVFXBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, gridWidth, sizeof(float) * 3);
        dammVFX.SetGraphicsBuffer("OffsetPositions", dammVFXBuffer);
        gridFloorBuffer = new ComputeBuffer(gridWidth * gridHeight, sizeof(int), ComputeBufferType.Structured);
        gridFloorBufferArray = new int[gridWidth * gridHeight];

        wallDistance = gridHeight;
        
        // Vector3 middleGrid = new Vector3(gridWidth / 2f, 1, gridHeight / 2f);
        // bounds = new Bounds(new Vector3(gridWidth / 2f, 0, gridHeight / 2f), middleGrid * 1000);
        
        tiles = new TileWrapper[AmountTileIDs];
        floorTiles = new FloorTileWrapper[AmountTileIDs];
        floorTiles[(int)FloorTileID.DIRT] = dirtTile;
        floorTiles[(int)FloorTileID.GRASS] = grassTile;
        floorTiles[(int)FloorTileID.WATER] = waterTile;
        floorTiles[(int)FloorTileID.PAVEMENT] = pavementTile;
        
        tiles[(int)EntityTileID.TREE] = treeTile;
        tiles[(int)EntityTileID.DAMM] = damTile;
        tiles[(int)EntityTileID.CLIFF] = cliffTile;

        matricesList = new List<List<Matrix4x4>>(tiles.Length);
        for (int i = 0; i < tiles.Length; i++)
        {
            matricesList.Add(new List<Matrix4x4>());
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
        {
            int indexPosToIndex = GridHelper.IndexPosToIndex(new Vector2Int(x, y));
            int tileID = gridObject.tiles[indexPosToIndex];
            int floorTileID = gridObject.floorTiles[indexPosToIndex];
            
            cachedIndex.x = x;
            cachedIndex.y = y;
            Matrix4x4 matrix4X4 = GridHelper.IndexToMatrix4x4(cachedIndex);
            tileIDs[indexPosToIndex] = (EntityTileID)tileID;
            
            gridFloorBufferArray[indexPosToIndex] = floorTileID;
            if(floorTileID == (int)FloorTileID.WATER)
                waterSpots.Add(cachedIndex);
            
            if(tileID == 0)
                continue;
            matricesList[tileID].Add(matrix4X4);
        }
        waterSpots = waterSpots.OrderBy(x => x.y).ToList();

        renderParamsArray = new RenderParams[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
        {
            bool copied = false;
            for (int j = 0; j < tiles.Length; j++)
            {
                if (tiles[i].renderSettings.material == tiles[j].renderSettings.material && i > j)
                {
                    renderParamsArray[i] = renderParamsArray[j];
                    copied = true;
                    break;
                }
            }
            
            if(copied)
                continue;

            if (tiles[i].renderSettings.material == null)
            {
                Debug.LogError($"Material at index {i} in TileSettings is not set");
            }
            
            RenderParams renderParams = new RenderParams(tiles[i].renderSettings.material)
            {
                matProps = new MaterialPropertyBlock(),
            };
            renderParamsArray[i] = renderParams;
        }

        gridFloorBuffer.SetData(gridFloorBufferArray);
        floorMat.SetBuffer(GridFloorBuffer, gridFloorBuffer);
        floorMat.SetFloat(GridWidth, gridWidth);
        floorMat.SetFloat(GridHeight, gridHeight);
        floorMat.SetFloat(TileSize, tileSize);

        Array.Fill(gridSelectionBufferArray, Vector4.one);
        gridSelectionBuffer.SetData(gridSelectionBufferArray);
        instanceMat.SetBuffer(GridBuffer, gridSelectionBuffer);
        
        instanceMat.SetFloat(GridWidth, gridWidth);
        instanceMat.SetFloat(GridHeight, gridHeight);
        instanceMat.SetFloat(TileSize, tileSize);
        
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


    private int cachedIndex;
    private EntityTileID cachedEntityTileID;
    private FloorTileID cachedfloorTileID;
    private void StartChangingTile()
    {
        cachedIndex = -1;
    }

    private void PlacementSelection(Vector3 position, EntityTileID entityTileID)
    {
        FloorTileID floorTileID = tiles[(int)entityTileID].floorID;
        Vector2Int posIndex = GridHelper.WorldPosToIndexPos(position);
        if(posIndex.x < 0 || posIndex.x >= gridWidth || posIndex.y < 0 || posIndex.y >= gridHeight)
            return;

        if (cachedIndex != -1)
        {
            ChangeBothTile(cachedIndex, cachedEntityTileID, cachedfloorTileID);
        }

        int index = GridHelper.IndexPosToIndex(posIndex);
        cachedEntityTileID = tileIDs[index];
        cachedfloorTileID = floorTileIDs[index];

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
        
        ChangeBothTile(index, entityTileID, floorTileID);
        
        gridSelectionBuffer.SetData(gridSelectionBufferArray);
        Array.Fill(gridSelectionBufferArray, Vector4.one);
    }
    
    private void TryChangeBothTile(Vector3 position, EntityTileID entityTileID, FloorTileID floorTileID)
    {
        Array.Fill(gridSelectionBufferArray, Vector4.one);
        gridSelectionBuffer.SetData(gridSelectionBufferArray);
        
        Vector2Int posIndex = GridHelper.WorldPosToIndexPos(position);
        if(posIndex.x < 0 || posIndex.x >= gridWidth || posIndex.y < 0 || posIndex.y >= gridHeight)
            return;
        
        EntityTileID oldEntityTile = tileIDs[GridHelper.IndexPosToIndex(posIndex)];
        
        bool requiredPlacement = GetRequiredSelection(entityTileID, posIndex);
        bool constrained = GetConstraintSelection(entityTileID, posIndex);
        bool hasEnoughApples = amountApples >= tiles[(int)entityTileID].TileGameSettings.appleCost;
        
        if (oldEntityTile == entityTileID || !requiredPlacement || !constrained || !hasEnoughApples)
            return;

        ChangeBothTile(position, entityTileID, floorTileID);

        GainApples(-tiles[(int)entityTileID].TileGameSettings.appleCost, position);
        EventSystem<int>.RaiseEvent(EventType.AMOUNT_APPLES, amountApples);
    }
    
    private void TryChangeBothTile(Vector3 position, EntityTileID entityTileID)
    {
        Array.Fill(gridSelectionBufferArray, Vector4.one);
        gridSelectionBuffer.SetData(gridSelectionBufferArray);
        
        Vector2Int posIndex = GridHelper.WorldPosToIndexPos(position);
        if(posIndex.x < 0 || posIndex.x >= gridWidth || posIndex.y < 0 || posIndex.y >= gridHeight)
            return;
        
        EntityTileID oldEntityTile = tileIDs[GridHelper.IndexPosToIndex(posIndex)];
        
        bool requiredPlacement = GetRequiredSelection(entityTileID, posIndex);
        bool constrained = GetConstraintSelection(entityTileID, posIndex);
        bool hasEnoughApples = amountApples >= tiles[(int)entityTileID].TileGameSettings.appleCost;
        
        if (oldEntityTile == entityTileID || !requiredPlacement || !constrained || !hasEnoughApples)
            return;

        ChangeBothTile(position, entityTileID, tiles[(int)entityTileID].floorID);

        GainApples(-tiles[(int)entityTileID].TileGameSettings.appleCost, position);
        EventSystem<int>.RaiseEvent(EventType.AMOUNT_APPLES, amountApples);
    }
    
    private void TryChangeEntityTile(Vector3 position, EntityTileID entityTileID)
    {
        Array.Fill(gridSelectionBufferArray, Vector4.one);
        gridSelectionBuffer.SetData(gridSelectionBufferArray);
        
        Vector2Int posIndex = GridHelper.WorldPosToIndexPos(position);
        if(posIndex.x < 0 || posIndex.x >= gridWidth || posIndex.y < 0 || posIndex.y >= gridHeight)
            return;
        
        EntityTileID oldEntityTile = tileIDs[GridHelper.IndexPosToIndex(posIndex)];
        
        bool requiredPlacement = GetRequiredSelection(entityTileID, posIndex);
        bool constrained = GetConstraintSelection(entityTileID, posIndex);
        bool hasEnoughApples = amountApples >= tiles[(int)entityTileID].TileGameSettings.appleCost;
        
        if (oldEntityTile == entityTileID || !requiredPlacement || !constrained || !hasEnoughApples)
            return;
        
        ChangeEntityTile(GridHelper.IndexPosToIndex(posIndex), entityTileID);

        GainApples(-tiles[(int)entityTileID].TileGameSettings.appleCost, position);
        EventSystem<int>.RaiseEvent(EventType.AMOUNT_APPLES, amountApples);
    }
    
    private void ChangeBothTile(Vector3 position, EntityTileID entityTileID, FloorTileID floorTileID)
    {
        int index = GridHelper.IndexPosToIndex(GridHelper.WorldPosToIndexPos(position));
        ChangeFloorTile(index, floorTileID);
        ChangeEntityTile(index, entityTileID);
    }
    private void ChangeBothTile(Vector2Int indexPos, EntityTileID entityTileID, FloorTileID floorTileID)
    {
        int index = GridHelper.IndexPosToIndex(indexPos);
        ChangeFloorTile(index, floorTileID);
        ChangeEntityTile(index, entityTileID);
    }
    private void ChangeBothTile(int index, EntityTileID entityTileID, FloorTileID floorTileID)
    {
        ChangeFloorTile(index, floorTileID);
        ChangeEntityTile(index, entityTileID);
    }
    private void ChangeEntityTile(int index, EntityTileID entityTileID)
    {
        if (entityTileID == 0)
            return;
        
        EntityTileID oldEntityTile = tileIDs[index];
        Matrix4x4 matrix4X4 = GridHelper.IndexToMatrix4x4(index);
        
        tileIDs[index] = entityTileID;
        matricesList[(int)oldEntityTile].RemoveSwapBack(matrix4X4);
        matricesList[(int)entityTileID].Add(matrix4X4);
    }
    private void ChangeEntityTile(Vector3 position, EntityTileID entityTileID)
    {
        int index = GridHelper.IndexPosToIndex(GridHelper.WorldPosToIndexPos(position));
        if (entityTileID == 0)
            return;
        
        EntityTileID oldEntityTile = tileIDs[index];
        Matrix4x4 matrix4X4 = GridHelper.IndexToMatrix4x4(index);
        
        tileIDs[index] = entityTileID;
        matricesList[(int)oldEntityTile].RemoveSwapBack(matrix4X4);
        matricesList[(int)entityTileID].Add(matrix4X4);
    }

    private void ChangeFloorTile(int index, FloorTileID floorTileID)
    {
        floorTileIDs[index] = floorTileID;
        gridFloorBufferArray[index] = (int)floorTileID;
        gridFloorBuffer.SetData(gridFloorBufferArray);
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
        floorMat.SetBuffer(GridFloorBuffer, gridFloorBuffer);
        floorMat.SetFloat(GridWidth, gridWidth);
        floorMat.SetFloat(GridHeight, gridHeight);
        floorMat.SetFloat(TileSize, tileSize);

        for (int i = 0; i < tiles.Length; i++)
        {
            if (matricesList[i].Count == 0)
                continue;

            foreach (TextureWtihReference textureWtihReference in tiles[i].renderSettings.textures)
            {
                renderParamsArray[i].matProps.SetTexture(textureWtihReference.textureName, textureWtihReference.texture);
            }
            
            Graphics.RenderMeshInstanced(renderParamsArray[i], tiles[i].renderSettings.mesh, 0, matricesList[i]);
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
                
                if(appleTrees[index] >= maxAmountApplesProduced)
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
            if ((tileIDs[index] == EntityTileID.DAMM || gridFloorBufferArray[index] == (int)FloorTileID.WATER) && damms[index].progress < 1.5f && damms[index].buildDamm)
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
            
            for (int x = 0; x < gridWidth; x++)
            {
                int dammIndex = GridHelper.IndexPosToIndex(new Vector2Int(x, wallDistance));
                if (damms[dammIndex].buildDamm && tileIDs[dammIndex] == EntityTileID.DAMM)
                {
                    damms[dammIndex].amountBeavorsWorking = 0;
                    damms[dammIndex].progress = 0;
                    damms[dammIndex].buildDamm = false;
                }
                
                ChangeBothTile(new Vector2Int(x, wallDistance), EntityTileID.EMPTY, FloorTileID.PAVEMENT);
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
                    foreach (EntityTileID t in placementConstraint.tileIDs)
                    {
                        if (tileIDs[GridHelper.IndexPosToIndex(cachedIndex)] != t)
                            continue;

                        int index = cachedIndex.x * gridWidth + cachedIndex.y % gridHeight;
                        callback?.Invoke(index);
                        meetsRequirements = false;
                        break;
                    }
                    foreach (FloorTileID t in placementConstraint.floorTileIDs)
                    {
                        if (floorTileIDs[GridHelper.IndexPosToIndex(cachedIndex)] != t)
                            continue;

                        int index = cachedIndex.x * gridWidth + cachedIndex.y % gridHeight;
                        callback?.Invoke(index);
                        meetsRequirements = false;
                        break;
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
                    foreach (EntityTileID t in areaRequirement.tileIDs)
                    {
                        if (tileIDs[GridHelper.IndexPosToIndex(cachedIndex)] != t)
                            continue;

                        requirementTileAmount++;
                        int index = cachedIndex.x * gridWidth + cachedIndex.y % gridHeight;
                        callback?.Invoke(index);
                        break;
                    }
                    foreach (FloorTileID t in areaRequirement.floorTileIDs)
                    {
                        if (floorTileIDs[GridHelper.IndexPosToIndex(cachedIndex)] != t)
                            continue;

                        int index = cachedIndex.x * gridWidth + cachedIndex.y % gridHeight;
                        callback?.Invoke(index);
                        meetsRequirements = false;
                        break;
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

















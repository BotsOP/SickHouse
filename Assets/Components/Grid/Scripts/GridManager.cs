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
    private readonly static int GridBuffer = Shader.PropertyToID("gridBuffer");
    private readonly static int GridWidth = Shader.PropertyToID("gridWidth");
    private readonly static int GridHeight = Shader.PropertyToID("gridHeight");
    private readonly static int TileSize = Shader.PropertyToID("tileSize");
    private readonly static int SelectionColor = Shader.PropertyToID("_SelectionColor");
    private readonly static int AlbedoMap = Shader.PropertyToID("_AlbedoMap");
    private readonly static int GridFloorBuffer = Shader.PropertyToID("_GridFloorBuffer");

    public TileID[] tileIDs;
    public int wallDistance;
    [NonSerialized] public List<List<Matrix4x4>> matricesList;
    
    [Foldout("Grid Settings")]
    [Header("Grid")]
    public int gridWidth = 100;
    public int gridHeight = 100;
    public float tileSize = 1;
    public List<GameObject> racoons = new List<GameObject>();
    public List<GameObject> beavers = new List<GameObject>();
    [SerializeField] private GridObject gridObject;
    [SerializeField] private TileObject tileObject;
    [SerializeField] private Material instanceMat;
    [SerializeField] private Material floorMat;
    [SerializeField] private int amountApples = 100;

    [Header("Selection")]
    [SerializeField] private GameObject selectionObject;
    [SerializeField] private Color constrainedColor = Color.red;
    [SerializeField] private Color placeableColor = Color.green;
    [SerializeField] private Color requirementColor = Color.blue;
    
    [Header("Misc")]
    [SerializeField] private TileID startFillTileID;

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
    
    private ComputeBuffer gridBuffer;
    private Vector4[] gridBufferArray;
    private ComputeBuffer gridFloorBuffer;
    private float[] gridFloorBufferArray;
    private GraphicsBuffer dammVFXBuffer;
    private MaterialPropertyBlock materialPropertyBlock;
    private TileID beforeSelectionTileID;
    private MeshFilter selectionMeshFilter;
    private MeshRenderer selectionMeshRenderer;
    private List<Vector3> dammVFXPositions;

    private float lastTimeAppleCycle;
    private float lastTimeWallCycle;

    private float bulldozerProgress = 0;
    private int amountDamsAgainstWall = 0;
    private int previousSkyscraperIndex = 0;
    private Damm[] damms;
    private int[] appleTrees;
    private RenderParams[] renderParamsArray;


    private void OnDisable()
    {
        gridBuffer?.Release();
        dammVFXBuffer?.Release();
        gridFloorBuffer?.Release();
        
        EventSystem<Vector3, TileID>.Unsubscribe(EventType.SELECT_TILE_DOWN, StartChangingTile);
        EventSystem<Vector3, TileID>.Unsubscribe(EventType.SELECT_TILE, PlacementSelection);
        EventSystem<Vector3, TileID>.Unsubscribe(EventType.CHANGE_TILE, ChangeTile);
        EventSystem<Vector3, TileID>.Unsubscribe(EventType.FORCE_CHANGE_TILE, ForceChangeTile);
        EventSystem.Unsubscribe(EventType.SPAWN_RACOON, SpawnRacoon);
        EventSystem.Unsubscribe(EventType.SPAWN_BEAVOR, SpawnBeavor);
        EventSystem<int, Vector3>.Unsubscribe(EventType.GAIN_APPLES, GainApples);
        EventSystem<int>.Unsubscribe(EventType.COLLECTED_APPLE, collectedAppleFromTree);
    }

    private void Awake()
    {
        EventSystem<Vector3, TileID>.Subscribe(EventType.SELECT_TILE_DOWN, StartChangingTile);
        EventSystem<Vector3, TileID>.Subscribe(EventType.SELECT_TILE, PlacementSelection);
        EventSystem<Vector3, TileID>.Subscribe(EventType.CHANGE_TILE, ChangeTile);
        EventSystem<Vector3, TileID>.Subscribe(EventType.FORCE_CHANGE_TILE, ForceChangeTile);
        EventSystem.Subscribe(EventType.SPAWN_RACOON, SpawnRacoon);
        EventSystem.Subscribe(EventType.SPAWN_BEAVOR, SpawnBeavor);
        EventSystem<int, Vector3>.Subscribe(EventType.GAIN_APPLES, GainApples);
        EventSystem<int>.Subscribe(EventType.COLLECTED_APPLE, collectedAppleFromTree);
        
        tileIDs = new TileID[gridWidth * gridHeight];
        gridBuffer = new ComputeBuffer(gridWidth * gridHeight, sizeof(float) * 4);
        gridBufferArray = new Vector4[gridWidth * gridHeight];
        appleTrees = new int[gridWidth * gridHeight];
        dammVFXPositions = new List<Vector3>(gridWidth);
        dammVFXBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, gridWidth, sizeof(float) * 3);
        dammVFX.SetGraphicsBuffer("OffsetPositions", dammVFXBuffer);
        gridFloorBuffer = new ComputeBuffer(gridWidth * gridHeight, sizeof(int), ComputeBufferType.Structured);
        gridFloorBufferArray = new float[gridWidth * gridHeight];

        wallDistance = gridHeight;
        
        // Vector3 middleGrid = new Vector3(gridWidth / 2f, 1, gridHeight / 2f);
        // bounds = new Bounds(new Vector3(gridWidth / 2f, 0, gridHeight / 2f), middleGrid * 1000);
        
        selectionMeshFilter = selectionObject.GetComponent<MeshFilter>();
        selectionMeshRenderer = selectionObject.GetComponent<MeshRenderer>();

        matricesList = new List<List<Matrix4x4>>(tileObject.tileSettings.Length);
        for (int i = 0; i < tileObject.tileSettings.Length; i++)
        {
            matricesList.Add(new List<Matrix4x4>());
        }

        bool gridObjectIsNull = gridObject is null;
        gridObject?.Load();
        
        Vector2Int cachedIndex = new Vector2Int(0, 0);
        for (int x = 0; x < gridWidth; x++)
        for (int y = 0; y < gridHeight; y++)
        {
            int tileID;
            int indexPosToIndex = IndexPosToIndex(new Vector2Int(x, y));
            if (gridObjectIsNull)
            {
                tileID = (int)startFillTileID;
            }
            else
            {
                tileID = gridObject.tiles[indexPosToIndex];
            }

            
            cachedIndex.x = x;
            cachedIndex.y = y;
            Matrix4x4 matrix4X4 = IndexToMatrix4x4(cachedIndex);
            matricesList[tileID].Add(matrix4X4);
            gridFloorBufferArray[indexPosToIndex] = (int)tileObject.tileSettings[tileID].floorID;
            tileIDs[indexPosToIndex] = (TileID)tileID;
        }
        matricesList[(int)TileID.WATER] = matricesList[(int)TileID.WATER].OrderBy(x => x.GetRow(2).w).ToList();

        renderParamsArray = new RenderParams[tileObject.tileSettings.Length];
        for (int i = 0; i < tileObject.tileSettings.Length; i++)
        {
            bool copied = false;
            for (int j = 0; j < tileObject.tileSettings.Length; j++)
            {
                if (tileObject.tileSettings[i].material == tileObject.tileSettings[j].material && i > j)
                {
                    renderParamsArray[i] = renderParamsArray[j];
                    copied = true;
                    break;
                }
            }
            
            if(copied)
                continue;

            if (tileObject.tileSettings[i].material == null)
            {
                Debug.LogError($"Material at index {i} in TileSettings is not set");
            }
            
            RenderParams renderParams = new RenderParams(tileObject.tileSettings[i].material)
            {
                matProps = new MaterialPropertyBlock(),
            };
            renderParamsArray[i] = renderParams;
        }
        
        // materialPropertyBlock = new MaterialPropertyBlock();
        // renderParams.matProps = materialPropertyBlock;
        gridFloorBuffer.SetData(gridFloorBufferArray);
        floorMat.SetBuffer(GridFloorBuffer, gridFloorBuffer);
        floorMat.SetFloat(GridWidth, gridWidth);
        floorMat.SetFloat(GridHeight, gridHeight);
        floorMat.SetFloat(TileSize, tileSize);

        Array.Fill(gridBufferArray, Vector4.one);
        gridBuffer.SetData(gridBufferArray);
        instanceMat.SetBuffer(GridBuffer, gridBuffer);
        
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

    private void StartChangingTile(Vector3 position, TileID tileID)
    {
        selectionObject.SetActive(true);
        selectionObject.transform.position = new Vector3(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z));
        selectionMeshFilter.sharedMesh = tileObject.tileSettings[(int)tileID].mesh;
    }

    private void PlacementSelection(Vector3 position, TileID tileID)
    {
        Vector2Int posIndex = GetTile(position);
        if(posIndex.x < 0 || posIndex.x >= gridWidth || posIndex.y < 0 || posIndex.y >= gridHeight)
            return;

        TileID oldTile = tileIDs[IndexPosToIndex(posIndex)];
        
        void AreaSelection(int index) { gridBufferArray[index] = placeableColor; }
        GetAreaSelection(tileID, posIndex, AreaSelection);

        int amountMatchingRequiredTiles = 0;
        void RequiredSelection(int index) { 
            gridBufferArray[index] = requirementColor;
            amountMatchingRequiredTiles++;
        }
        bool requiredPlacement = GetRequiredSelection(tileID, posIndex, RequiredSelection);

        void ConstraintSelection(int index) { 
            gridBufferArray[index] = constrainedColor;
        }
        bool constrained = GetConstraintSelection(tileID, posIndex, ConstraintSelection);
        
        selectionMeshRenderer.material.SetVector(SelectionColor, placeableColor);
        if (!requiredPlacement)
        {
            selectionMeshRenderer.material.SetVector(SelectionColor, requirementColor);
        }
        if (!constrained || oldTile == tileID)
        {
            selectionMeshRenderer.material.SetVector(SelectionColor, constrainedColor);
        }

        if (tileID != beforeSelectionTileID)
        {
            beforeSelectionTileID = tileID;
            selectionMeshRenderer.material.SetTexture(AlbedoMap, tileObject.tileSettings[(int)tileID].texture);
        }

        if (tileID == TileID.TREE)
        {
            int amountRequiredTiles = tileObject.tileSettings[(int)TileID.TREE].placementRequirements[0].amountRequiredTiles;
            float lockedX = position.x > 0 ? (int)(position.x + tileSize / 2) : (int)(position.x - tileSize / 2);
            float lockedZ = position.z > 0 ? (int)(position.z + tileSize / 2) : (int)(position.z - tileSize / 2);
            Vector3 lockedPosition = new Vector3(lockedX, 0, lockedZ);
            EventSystem<int, int, Color, Vector3>.RaiseEvent(EventType.UPDATE_SELECTION_TEXT, 
                                                             amountMatchingRequiredTiles, 
                                                             amountRequiredTiles, 
                                                             amountMatchingRequiredTiles>=amountRequiredTiles?placeableColor:requirementColor, 
                                                             lockedPosition);
        }
        
        selectionObject.transform.position = new Vector3(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z));
        
        int index = posIndex.x * gridWidth + posIndex.y % gridHeight;
        gridBufferArray[index].w = 0;
        
        gridBuffer.SetData(gridBufferArray);
        Array.Fill(gridBufferArray, Vector4.one);
    }
    
    private void ChangeTile(Vector3 position, TileID tileID)
    {
        Array.Fill(gridBufferArray, Vector4.one);
        gridBuffer.SetData(gridBufferArray);
        
        Vector2Int posIndex = GetTile(position);
        if(posIndex.x < 0 || posIndex.x >= gridWidth || posIndex.y < 0 || posIndex.y >= gridHeight)
            return;
        
        TileID oldTile = tileIDs[IndexPosToIndex(posIndex)];
        selectionObject.SetActive(false);
        
        bool requiredPlacement = GetRequiredSelection(tileID, posIndex);
        bool constrained = GetConstraintSelection(tileID, posIndex);
        bool hasEnoughApples = amountApples >= tileObject.tileSettings[(int)tileID].appleCost || tileID == TileID.DIRT;
        
        if (oldTile == tileID || !requiredPlacement || !constrained || !hasEnoughApples)
            return;
        
        Matrix4x4 matrix4X4 = IndexToMatrix4x4(posIndex);
        
        tileIDs[IndexPosToIndex(posIndex)] = tileID;
        if (oldTile == TileID.WATER)
        {
            matricesList[(int)oldTile].Remove(matrix4X4);
        }
        else
        {
            matricesList[(int)oldTile].RemoveSwapBack(matrix4X4);
        }
        matricesList[(int)tileID].Add(matrix4X4);

        switch (tileID)
        {
            case TileID.WATER:
                matricesList[(int)TileID.WATER] = matricesList[(int)TileID.WATER].OrderBy(x => x.GetRow(2).w).ToList();
                break;

            case TileID.DIRT:
                GainApples(tileObject.tileSettings[(int)oldTile].appleCost, position);
                return;
        }

        GainApples(-tileObject.tileSettings[(int)tileID].appleCost, position);
        EventSystem<int>.RaiseEvent(EventType.AMOUNT_APPLES, amountApples);
    }
    
    private void ForceChangeTile(Vector3 position, TileID tileID)
    {
        Vector2Int posIndex = GetTile(position);
        TileID oldTile = tileIDs[IndexPosToIndex(posIndex)];
        Matrix4x4 matrix4X4 = IndexToMatrix4x4(posIndex);

        if(oldTile == TileID.DAMM_WATER && tileID == TileID.DAMM)
            return;
        
        if (oldTile == TileID.WATER && tileID == TileID.DAMM)
            tileID = TileID.DAMM_WATER;
        
        tileIDs[IndexPosToIndex(posIndex)] = tileID;
        matricesList[(int)oldTile].RemoveSwapBack(matrix4X4);
        matricesList[(int)tileID].Add(matrix4X4);
    }
    private void ForceChangeTile(Vector2Int posIndex, TileID tileID)
    {
        TileID oldTile = tileIDs[IndexPosToIndex(posIndex)];
        
        Matrix4x4 matrix4X4 = IndexToMatrix4x4(posIndex);
        
        tileIDs[IndexPosToIndex(posIndex)] = tileID;
        matricesList[(int)oldTile].RemoveSwapBack(matrix4X4);
        matricesList[(int)tileID].Add(matrix4X4);
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
        int amountDams = matricesList[(int)TileID.DAMM_WATER].Count;
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

        for (int i = 0; i < tileObject.tileSettings.Length; i++)
        {
            if (matricesList[i].Count == 0)
            {
                continue;
            }
            renderParamsArray[i].matProps.SetTexture(AlbedoMap, tileObject.tileSettings[i].texture);
            Graphics.RenderMeshInstanced(renderParamsArray[i], tileObject.tileSettings[i].mesh, 0, matricesList[i]);
        }

        UpdateApples();
        
        UpdateWall();

        int amountDams = matricesList[(int)TileID.DAMM_WATER].Count;
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
            for (int i = 0; i < matricesList[(int)TileID.TREE].Count; i++)
            {
                Vector3 position = matricesList[(int)TileID.TREE][i].GetPosition();
                int index = IndexPosToIndex(GetTile(position));
                
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
            int index = IndexPosToIndex(posIndex);
            if ((tileIDs[index] == TileID.DAMM || tileIDs[index] == TileID.DAMM_WATER) && damms[index].progress < 1.5f && damms[index].buildDamm)
            {
                amountDamsAgainstWall++;
                dammVFXPositions.Add(GetPosition(posIndex));
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
                int dammIndex = IndexPosToIndex(new Vector2Int(x, wallDistance));
                if (damms[dammIndex].buildDamm && tileIDs[dammIndex] == TileID.DAMM)
                {
                    damms[dammIndex].amountBeavorsWorking = 0;
                    damms[dammIndex].progress = 0;
                    damms[dammIndex].buildDamm = false;
                }
                
                ForceChangeTile(new Vector2Int(x, wallDistance), TileID.WALL);
            }
        }
    }


    private Vector2Int GetTile(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt((worldPos.x / tileSize) + (gridWidth * tileSize / 2f)), Mathf.RoundToInt((worldPos.z / tileSize) + (gridHeight * tileSize / 2f)));
    }

    private Vector2Int GetTile(Matrix4x4 matrix)
    {
        return GetTile(new Vector3(matrix.GetRow(0).w, 0, matrix.GetRow(2).w));
    }
    
    private Vector3 GetPosition(Vector2Int index)
    {
        return new Vector3((index.x * tileSize) - (gridWidth / tileSize / 2f), 0, (index.y * tileSize) - (gridHeight / tileSize / 2f));
    }
    
    private void GetAreaSelection(TileID tileID, Vector2Int posIndex, Action<int> callback)
    {
        Vector2Int cachedIndex = new Vector2Int();
        foreach (AreaSelection areaSelection in tileObject.tileSettings[(int)tileID].selection)
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
    
    private bool GetConstraintSelection(TileID tileID, Vector2Int posIndex, Action<int> callback = null)
    {
        Vector2Int cachedIndex = new Vector2Int();
        bool meetsRequirements = true;
        foreach (AreaConstraint placementConstraint in tileObject.tileSettings[(int)tileID].placementConstraints)
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
                    foreach (TileID t in placementConstraint.tileID)
                    {
                        if (tileIDs[IndexPosToIndex(cachedIndex)] != t)
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
    
    private bool GetRequiredSelection(TileID tileID, Vector2Int posIndex, Action<int> callback = null)
    {
        Vector2Int cachedIndex = new Vector2Int();
        bool meetsRequirements = true;
        foreach (AreaRequirement areaRequirement in tileObject.tileSettings[(int)tileID].placementRequirements)
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
                    foreach (TileID t in areaRequirement.tileID)
                    {
                        if (tileIDs[IndexPosToIndex(cachedIndex)] != t)
                            continue;

                        requirementTileAmount++;
                        int index = cachedIndex.x * gridWidth + cachedIndex.y % gridHeight;
                        callback?.Invoke(index);
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
    
    private void GainApples(int amount, Vector3 worldPos)
    {
        amountApples += amount;
        EventSystem<int, Vector3>.RaiseEvent(EventType.CHANGE_AMOUNT_APPLES, amount, worldPos);
    }
    private void collectedAppleFromTree(int treeIndex)
    {
        appleTrees[treeIndex]--;
    }
    private int IndexPosToIndex(Vector2Int index)
    {
        return index.x * gridWidth + index.y % gridHeight;
    }
    private Matrix4x4 IndexToMatrix4x4(Vector2Int index)
    {
        Vector3 position = new Vector3(index.x * tileSize, 0.0f, index.y * tileSize) - new Vector3(gridWidth * tileSize / 2f, 0, gridHeight * tileSize / 2f);
        Matrix4x4 matrix4X4 = Matrix4x4.Translate(position) * Matrix4x4.Scale(new Vector3(tileSize, tileSize, tileSize));
        return matrix4X4;
    }
}

















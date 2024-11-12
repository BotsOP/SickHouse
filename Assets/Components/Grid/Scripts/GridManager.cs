using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;


public class GridManager : MonoBehaviour
{
    private readonly static int GridBuffer = Shader.PropertyToID("gridBuffer");
    private readonly static int GridWidth = Shader.PropertyToID("gridWidth");
    private readonly static int GridHeight = Shader.PropertyToID("gridHeight");
    private readonly static int TileSize = Shader.PropertyToID("tileSize");
    
    [SerializeField] private ComputeShader computeShader;
    [SerializeField] private TileObject tileObject;
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;
    [SerializeField] private float tileSize = 1;
    [SerializeField] private int gridWidth = 100;
    [SerializeField] private int gridHeight = 100;
    [SerializeField] private Color constrainedColor = Color.red;
    [SerializeField] private Color placeableColor = Color.green;
    [SerializeField] private Color requirementColor = Color.blue;

    [SerializeField] private GameObject selectionObject;
    
    private TileID[,] tiles;
    private ComputeBuffer gridBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = { 0, 0, 0, 0, 0 };
    private Bounds bounds;
    private List<List<Matrix4x4>> matricesList;
    private RenderParams renderParams;
    private RenderParams selectionParams;
    private MaterialPropertyBlock materialPropertyBlock;
    private Vector4[] selectionColors;
    private Vector2Int beforeSelectionIndex;
    private TileID beforeSelectionTileID;
    private MeshFilter selectionMeshFilter;
    private MeshRenderer selectionMeshRenderer;


    private void OnDisable()
    {
        gridBuffer?.Release();
        argsBuffer?.Release();
    }

    private void Awake()
    {
        tiles = new TileID[gridWidth, gridHeight];
        gridBuffer = new ComputeBuffer(gridWidth * gridHeight, sizeof(float) * 4);
        selectionColors = new Vector4[gridWidth * gridHeight];
        
        Vector3 middleGrid = new Vector3(gridWidth / 2f, 1, gridHeight / 2f);
        bounds = new Bounds(new Vector3(gridWidth / 2f, 0, gridHeight / 2f), middleGrid * 1000);
        
        selectionMeshFilter = selectionObject.GetComponent<MeshFilter>();
        selectionMeshRenderer = selectionObject.GetComponent<MeshRenderer>();

        matricesList = new List<List<Matrix4x4>>(tileObject.tiles.Length);
        for (int i = 0; i < tileObject.tiles.Length; i++)
        {
            matricesList.Add(new List<Matrix4x4>());
        }

        Vector2Int cachedIndex = new Vector2Int(0, 0);
        for (int x = 0; x < gridWidth; x++)
        for (int y = 0; y < gridHeight; y++)
        {
            int index = x * gridWidth + y % gridHeight;
            // int randomIndex = 0;
            int randomIndex = Random.Range(0, 2);
            selectionColors[index] = new Vector3(randomIndex, randomIndex, randomIndex);

            cachedIndex.x = x;
            cachedIndex.y = y;
            Matrix4x4 matrix4X4 = IndexToMatrix4x4(cachedIndex);
            matricesList[randomIndex].Add(matrix4X4);

            tiles[x, y] = (TileID)randomIndex;
        }

        materialPropertyBlock = new MaterialPropertyBlock();
        renderParams = new RenderParams(material);
        renderParams.matProps = materialPropertyBlock;

        Array.Fill(selectionColors, Vector3.one);
        gridBuffer.SetData(selectionColors);
        material.SetBuffer(GridBuffer, gridBuffer);
        
        material.SetFloat(GridWidth, gridWidth);
        material.SetFloat(GridHeight, gridHeight);
        material.SetFloat(TileSize, tileSize);
        
        // UpdateMaterial();

        // argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        // args[0] = mesh.GetIndexCount(0);
        // args[1] = (uint)(gridWidth * gridHeight);
        // argsBuffer.SetData(args);
        //
        // computeShader.SetBuffer(0, "gridBuffer", gridBuffer);
    }
    private Matrix4x4 IndexToMatrix4x4(Vector2Int index)
    {
        Vector3 position = new Vector3(index.x * tileSize, 0.0f, index.y * tileSize) - new Vector3(gridWidth * tileSize / 2f, 0, gridHeight * tileSize / 2f);
        Matrix4x4 matrix4X4 = Matrix4x4.Translate(position) * Matrix4x4.Scale(new Vector3(tileSize, tileSize, tileSize));
        return matrix4X4;
    }


    public void PlacementSelection(Vector3 position, TileID tileID)
    {
        // cachedSelectionMatrices.Clear();
        // Vector3 cachedPos = Vector3.zero;
        // Vector3 tileScale = new Vector3(tileSize, tileSize, tileSize);
        // position.x = Mathf.Round(position.x);
        // position.y = Mathf.Round(position.y);
        // position.z = Mathf.Round(position.z);
        // foreach (SelectionBox selectionBox in selectionBoxes)
        // {
        //     Vector2Int lowerCorner = new Vector2Int(Mathf.RoundToInt(selectionBox.position.x - selectionBox.size.x / 2f), Mathf.RoundToInt(selectionBox.position.z - selectionBox.size.z / 2f));
        //     Vector2Int upperCorner = new Vector2Int(Mathf.RoundToInt(selectionBox.position.x + selectionBox.size.x / 2f), Mathf.RoundToInt(selectionBox.position.z + selectionBox.size.z / 2f));
        //     for (int x = lowerCorner.x; x < upperCorner.x; x++)
        //     for (int y = lowerCorner.y; y < upperCorner.y; y++)
        //     {
        //         cachedPos.x = x;
        //         cachedPos.z = y;
        //         Matrix4x4 matrix4X4 = Matrix4x4.Translate(cachedPos + position) * Matrix4x4.Scale(tileScale);
        //         cachedSelectionMatrices.Add(matrix4X4);
        //     }
        // }
        //
        // if (selectionSpheres == null)
        // {
        //     return;
        // }
        //
        // foreach (SelectionSphere selectionSphere in selectionSpheres)
        // {
        //     Vector2Int lowerCorner = new Vector2Int(Mathf.RoundToInt(selectionSphere.position.x - selectionSphere.size / 2f), Mathf.RoundToInt(selectionSphere.position.z - selectionSphere.size / 2f));
        //     Vector2Int upperCorner = new Vector2Int(Mathf.RoundToInt(selectionSphere.position.x + selectionSphere.size / 2f), Mathf.RoundToInt(selectionSphere.position.z + selectionSphere.size / 2f));
        //     for (int x = lowerCorner.x; x < upperCorner.x; x++)
        //     for (int y = lowerCorner.y; y < upperCorner.y; y++)
        //     {
        //         if(Vector3.Distance(selectionSphere.position, new Vector3(x, selectionSphere.position.y, y)) > selectionSphere.size / 2)
        //             continue;
        //         
        //         cachedPos.x = x;
        //         cachedPos.z = y;
        //         Matrix4x4 matrix4X4 = Matrix4x4.Translate(cachedPos + position) * Matrix4x4.Scale(tileScale);
        //         cachedSelectionMatrices.Add(matrix4X4);
        //     }
        // }
        
        Vector2Int posIndex = GetTile(position);
        TileID oldTile = tiles[posIndex.x, posIndex.y];

        
        if (oldTile == tileID)
            return;
        
        void AreaSelection(int index) { selectionColors[index] = placeableColor; }
        GetAreaSelection(tileID, posIndex, AreaSelection);
        
        bool requiredPlacement = false;
        void RequiredSelection(int index) { 
            selectionColors[index] = requirementColor;
            requiredPlacement = true;
        }
        GetRequiredSelection(tileID, posIndex, RequiredSelection);

        bool constrained = false;
        void ConstraintSelection(int index) { 
            selectionColors[index] = constrainedColor;
            constrained = true;
        }
        GetConstraintSelection(tileID, posIndex, ConstraintSelection);
        
        selectionMeshRenderer.material.SetVector("_SelectionColor", placeableColor);
        if (requiredPlacement)
        {
            selectionMeshRenderer.material.SetVector("_SelectionColor", requirementColor);
        }
        if (constrained)
        {
            selectionMeshRenderer.material.SetVector("_SelectionColor", constrainedColor);
        }

        if (tileID != beforeSelectionTileID)
        {
            beforeSelectionTileID = tileID;
            selectionMeshRenderer.material.SetTexture("_AlbedoMap", tileObject.tiles[(int)tileID].texture);
        }
        
        if (beforeSelectionIndex != posIndex)
        {
            selectionObject.SetActive(true);
            selectionObject.transform.position = new Vector3(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z));
            selectionMeshFilter.sharedMesh = tileObject.tiles[(int)tileID].mesh;
            // selectionMeshRenderer.material.SetFloat();
            beforeSelectionIndex = posIndex;
        }
        int index = posIndex.x * gridWidth + posIndex.y % gridHeight;
        selectionColors[index].w = 0;
        
        gridBuffer.SetData(selectionColors);
        Array.Fill(selectionColors, Vector4.one);
    }
    
    

    private void Update()
    {
        // UpdateMaterial();
        for (int i = 0; i < tileObject.tiles.Length; i++)
        {
            if (matricesList[i].Count == 0)
            {
                continue;
            }
            materialPropertyBlock.SetTexture("_AlbedoMap", tileObject.tiles[i].texture);
            Graphics.RenderMeshInstanced(renderParams, tileObject.tiles[i].mesh, 0, matricesList[i]);
            // Graphics.RenderMeshIndirect(renderParams, tileObject.tiles[i].mesh, );
        }
    }

    public void ChangeTile(Vector3 position, TileID tileID)
    {
        Array.Fill(selectionColors, Vector4.one);
        gridBuffer.SetData(selectionColors);
        
        Vector2Int posIndex = GetTile(position);
        TileID oldTile = tiles[posIndex.x, posIndex.y];
        selectionObject.SetActive(false);
        
        bool requiredPlacement = false;
        void RequiredSelection(int index) { 
            selectionColors[index] = requirementColor;
            requiredPlacement = true;
        }
        GetRequiredSelection(tileID, posIndex, RequiredSelection);

        bool constrained = false;
        void ConstraintSelection(int index) { 
            selectionColors[index] = constrainedColor;
            constrained = true;
        }
        GetConstraintSelection(tileID, posIndex, ConstraintSelection);
        
        if (oldTile == tileID || requiredPlacement || constrained)
            return;
        
        Matrix4x4 matrix4X4 = IndexToMatrix4x4(posIndex);
        
        tiles[posIndex.x, posIndex.y] = tileID;
        matricesList[(int)oldTile].RemoveSwapBack(matrix4X4);
        matricesList[(int)tileID].Add(matrix4X4);
        // int index = posIndex.x * gridWidth + posIndex.y % gridHeight;
        // computeShader.SetInt("index", index);
        // computeShader.SetInt("tileID", (int)tileID);
        // computeShader.Dispatch(0, 1, 1, 1);
    }

    private Vector2Int GetTile(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt((worldPos.x / tileSize) + (gridWidth * tileSize / 2f)), Mathf.RoundToInt((worldPos.z / tileSize) + (gridHeight * tileSize / 2f)));
    }
    
    private Vector3 GetPosition(Vector2Int index)
    {
        return new Vector3(Mathf.RoundToInt((index.x * tileSize) - (gridWidth / tileSize * 2f)), 0, Mathf.RoundToInt((index.y * tileSize) - (gridHeight / tileSize * 2f)));
    }
    
    private void GetAreaSelection(TileID tileID, Vector2Int posIndex, Action<int> callback)
    {
        Vector2Int cachedIndex = new Vector2Int();
        foreach (AreaSelection areaSelection in tileObject.tiles[(int)tileID].selection)
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
                    cachedIndex.x = posIndex.x + x;
                    cachedIndex.y = posIndex.y + y;
                    int index = cachedIndex.x * gridWidth + cachedIndex.y % gridHeight;
                    callback?.Invoke(index);
                }
            }
        }
    }
    
    private void GetConstraintSelection(TileID tileID, Vector2Int posIndex, Action<int> callback)
    {
        Vector2Int cachedIndex = new Vector2Int();
        foreach (AreaRequirement placementConstraint in tileObject.tiles[(int)tileID].placementConstraints)
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
                    cachedIndex.x = posIndex.x + x;
                    cachedIndex.y = posIndex.y + y;
                    bool invoke = false;
                    foreach (TileID t in placementConstraint.tileID)
                    {
                        if (tiles[cachedIndex.x, cachedIndex.y] == t)
                            continue;

                        invoke = true;
                        break;
                    }
                    if (invoke)
                    {
                        int index = cachedIndex.x * gridWidth + cachedIndex.y % gridHeight;
                        callback?.Invoke(index);
                    }
                }
            }
        }
    }
    
    private void GetRequiredSelection(TileID tileID, Vector2Int posIndex, Action<int> callback)
    {
        Vector2Int cachedIndex = new Vector2Int();
        foreach (AreaRequirement areaRequirement in tileObject.tiles[(int)tileID].placementRequirements)
        {
            foreach (SelectionBox placementConstraintSelectionBox in areaRequirement.selectionBoxes)
            {
                Vector2Int lowerCorner = new Vector2Int(Mathf.CeilToInt(placementConstraintSelectionBox.position.x - placementConstraintSelectionBox.size.x / 2f), 
                                                        Mathf.CeilToInt(placementConstraintSelectionBox.position.y - placementConstraintSelectionBox.size.y / 2f));
                Vector2Int upperCorner = new Vector2Int(Mathf.CeilToInt(placementConstraintSelectionBox.position.x + placementConstraintSelectionBox.size.x / 2f), 
                                                        Mathf.CeilToInt(placementConstraintSelectionBox.position.y + placementConstraintSelectionBox.size.y / 2f));
                
                for (int x = lowerCorner.x; x < upperCorner.x; x++)
                for (int y = lowerCorner.y; y < upperCorner.y; y++)
                {
                    cachedIndex.x = posIndex.x + x;
                    cachedIndex.y = posIndex.y + y;
                    bool invoke = false;
                    foreach (TileID t in areaRequirement.tileID)
                    {
                        if (tiles[cachedIndex.x, cachedIndex.y] != t)
                            continue;

                        invoke = true;
                        break;
                    }
                    if (invoke)
                    {
                        int index = cachedIndex.x * gridWidth + cachedIndex.y % gridHeight;
                        callback?.Invoke(index);
                    }
                }
            }
        }
    }
    
    private struct GridTile
    {
        // public Vector3 position;
        public int tileID;
    }


}















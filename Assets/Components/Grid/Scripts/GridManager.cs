using System;
using UnityEngine;
using Random = UnityEngine.Random;


public class GridManager : MonoBehaviour
{
    private readonly static int GridBuffer = Shader.PropertyToID("gridBuffer");
    private readonly static int GridWidth = Shader.PropertyToID("gridWidth");
    private readonly static int GridHeight = Shader.PropertyToID("gridHeight");
    private readonly static int TileSize = Shader.PropertyToID("tileSize");
    private readonly static int TileRotation = Shader.PropertyToID("tileRotation");
    
    [SerializeField] private ComputeShader computeShader;
    [SerializeField] private TileObject tileObject;
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;
    [SerializeField] private float tileSize = 1;
    [SerializeField] private int gridWidth = 100;
    [SerializeField] private int gridHeight = 100;
    
    private TileID[,] tiles;
    private ComputeBuffer gridBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = { 0, 0, 0, 0, 0 };
    private Bounds bounds;

    private void OnDisable()
    {
        gridBuffer?.Release();
        argsBuffer?.Release();
    }

    private void Awake()
    {
        tiles = new TileID[gridWidth, gridHeight];
        gridBuffer = new ComputeBuffer(gridWidth * gridHeight, sizeof(float) * 3 + sizeof(int));
        
        Vector3 middleGrid = new Vector3(gridWidth / 2f, 1, gridHeight / 2f);
        bounds = new Bounds(new Vector3(gridWidth / 2f, 0, gridHeight / 2f), middleGrid * 1000);

        GridTile[] gridTiles = new GridTile[gridWidth * gridHeight];
        for (int x = 0; x < gridWidth; x++)
        for (int y = 0; y < gridHeight; y++)
        {
            int index = x * gridWidth + y % gridHeight;
            int randomIndex = Random.Range(0, tileObject.tiles.Length);

            gridTiles[index].position = new Vector3(x * tileSize, 0.0f, y * tileSize) - new Vector3(gridWidth * tileSize / 2f, 0, gridHeight * tileSize / 2f);
            gridTiles[index].tileID = randomIndex;

            tiles[x, y] = (TileID)randomIndex;
        }
        gridBuffer.SetData(gridTiles);
        
        UpdateMaterial();

        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        args[0] = mesh.GetIndexCount(0);
        args[1] = (uint)(gridWidth * gridHeight);
        argsBuffer.SetData(args);
        
        computeShader.SetBuffer(0, "gridBuffer", gridBuffer);
    }
    
    private void UpdateMaterial()
    {

        material.SetBuffer(GridBuffer, gridBuffer);
        material.SetFloat(GridWidth, gridWidth);
        material.SetFloat(GridHeight, gridHeight);
        material.SetFloat(TileSize, tileSize / 2);
        Quaternion rotation = transform.rotation;
        material.SetVector(TileRotation, new Vector4(rotation.x, rotation.y, rotation.z, rotation.w));
    }

    private void Update()
    {
        UpdateMaterial();
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
    }

    public void ChangeTile(Vector3 position, TileID tileID)
    {
        Vector2Int posIndex = GetTile(position);
        TileID oldTile = tiles[posIndex.x, posIndex.y];
        
        if (oldTile == tileID)
        {
            return;
        }
        
        Debug.Log($"Changed tile to {tileID}");
        tiles[posIndex.x, posIndex.y] = tileID;
        int index = posIndex.x * gridWidth + posIndex.y % gridHeight;
        computeShader.SetInt("index", index);
        computeShader.SetInt("tileID", (int)tileID);
        computeShader.Dispatch(0, 1, 1, 1);
    }

    private Vector2Int GetTile(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt((worldPos.x / tileSize) + (gridWidth * tileSize / 2f)), Mathf.RoundToInt((worldPos.z / tileSize) + (gridHeight * tileSize / 2f)));
    }
    
    private struct GridTile
    {
        public Vector3 position;
        public int tileID;
    }
}















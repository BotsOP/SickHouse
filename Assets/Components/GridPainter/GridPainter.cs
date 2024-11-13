using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using VInspector;
using EventType = Managers.EventType;
using Object = UnityEngine.Object;

public class GridPainter : MonoBehaviour
{
    [SerializeField] private GridObject gridObject;
    
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TileID tileID;
    private Vector3 cachedPosition;

    private void Update()
    {
        #region Switch_Tiles

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            tileID = 0;
            Debug.Log($"Changed tile to {tileID}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            tileID = (TileID)1;
            Debug.Log($"Changed tile to {tileID}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            tileID = (TileID)2;
            Debug.Log($"Changed tile to {tileID}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            tileID = (TileID)3;
            Debug.Log($"Changed tile to {tileID}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            tileID = (TileID)4;
            Debug.Log($"Changed tile to {tileID}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            tileID = (TileID)5;
            Debug.Log($"Changed tile to {tileID}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            tileID = (TileID)6;
            Debug.Log($"Changed tile to {tileID}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            tileID = (TileID)7;
            Debug.Log($"Changed tile to {tileID}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            tileID = (TileID)8;
            Debug.Log($"Changed tile to {tileID}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            tileID = (TileID)9;
            Debug.Log($"Changed tile to {tileID}");
        }

        #endregion
        
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Input.GetMouseButton(0))
        {
            if (Physics.Raycast(ray, out hit)) {
                cachedPosition = hit.point;
                EventSystem<Vector3, TileID>.RaiseEvent(EventType.FORCE_CHANGE_TILE, cachedPosition, tileID);
            }
        }
    }

    [Button]
    public void SaveGrid()
    {
        if (gridObject == null)
        {
            Debug.LogError($"GridObject is empty");
            return;
        }
        GridManager gridManager = FindFirstObjectByType<GridManager>();
        int[,] tiles = new int[gridManager.gridWidth,gridManager.gridHeight];
        Array.Copy(gridManager.tiles, tiles, gridManager.tiles.Length);
        gridObject.tiles = tiles;
        gridObject.Save();
    }
}

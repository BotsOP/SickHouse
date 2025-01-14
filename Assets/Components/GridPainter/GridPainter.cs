using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using VInspector;
using EventType = Managers.EventType;
using Object = UnityEngine.Object;

public class GridPainter : MonoBehaviour
{
    [SerializeField] private string fileName;
    
    [SerializeField] private Camera mainCamera;
    [SerializeField] private EntityTileID entityTileID;
    
    private Vector3 cachedPosition;

    private void Update()
    {
        #region Switch_Tiles

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            entityTileID = 0;
            Debug.Log($"Changed tile to {entityTileID}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            entityTileID = (EntityTileID)1;
            Debug.Log($"Changed tile to {entityTileID}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            entityTileID = (EntityTileID)2;
            Debug.Log($"Changed tile to {entityTileID}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            entityTileID = (EntityTileID)3;
            Debug.Log($"Changed tile to {entityTileID}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            entityTileID = (EntityTileID)4;
            Debug.Log($"Changed tile to {entityTileID}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            entityTileID = (EntityTileID)5;
            Debug.Log($"Changed tile to {entityTileID}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            entityTileID = (EntityTileID)6;
            Debug.Log($"Changed tile to {entityTileID}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            entityTileID = (EntityTileID)7;
            Debug.Log($"Changed tile to {entityTileID}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            entityTileID = (EntityTileID)8;
            Debug.Log($"Changed tile to {entityTileID}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            entityTileID = (EntityTileID)9;
            Debug.Log($"Changed tile to {entityTileID}");
        }

        #endregion
        
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Input.GetMouseButton(0))
        {
            if (Physics.Raycast(ray, out hit)) {
                cachedPosition = hit.point;
                EventSystem<Vector3, EntityTileID>.RaiseEvent(EventType.FORCE_CHANGE_TILE, cachedPosition, entityTileID);
            }
        }
    }

    [Button]
    public void SaveGrid()
    {
        GridManagerPainter gridManager = FindFirstObjectByType<GridManagerPainter>();
        GridObject.Save(gridManager.tileIDs, fileName);
    }
}

using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using EventType = Managers.EventType;

public class EditGrid : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TileID tileID;
    private Vector3 cachedPosition;

    private void OnEnable()
    {
        EventSystem<TileID>.Subscribe(EventType.CHANGE_BRUSH, ChangeTileID);
    }
    private void OnDisable()
    {
        EventSystem<TileID>.Unsubscribe(EventType.CHANGE_BRUSH, ChangeTileID);
    }

    private void ChangeTileID(TileID tileID)
    {
        this.tileID = tileID;
    }

    private void Update()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        
        if(isOverUI)
            return;
        
        if (Input.GetMouseButton(0))
        {
            if (Physics.Raycast(ray, out hit)) {
                cachedPosition = hit.point;
                EventSystem<Vector3, TileID>.RaiseEvent(EventType.SELECT_TILE, hit.point, tileID);
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            EventSystem<Vector3, TileID>.RaiseEvent(EventType.CHANGE_TILE, cachedPosition, tileID);
        }
    }
}

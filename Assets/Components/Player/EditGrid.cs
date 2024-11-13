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

    private void Update()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
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

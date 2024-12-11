using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using EventSystem = Managers.EventSystem;
using EventType = Managers.EventType;

public class EditGrid : MonoBehaviour
{
    [SerializeField] private bool disableInput;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private EntityTileID entityTileID;
    [SerializeField] private FloorTileID floorTileID;
    private bool activeEntityTileID;
    private Vector3 cachedPosition;
    private int UILayer;
    private bool didPressKeyDown = false;

    private void OnEnable()
    {
        UILayer = LayerMask.NameToLayer("UI");
        
        EventSystem<bool>.Subscribe(EventType.TOGGLE_INPUT, ToggleInput);
        EventSystem<EntityTileID>.Subscribe(EventType.CHANGE_BRUSH, ChangeTileID);
        EventSystem<FloorTileID>.Subscribe(EventType.CHANGE_BRUSH, ChangeTileID);
    }
    private void OnDisable()
    {
        EventSystem<bool>.Unsubscribe(EventType.TOGGLE_INPUT, ToggleInput);
        EventSystem<EntityTileID>.Unsubscribe(EventType.CHANGE_BRUSH, ChangeTileID);
        EventSystem<FloorTileID>.Unsubscribe(EventType.CHANGE_BRUSH, ChangeTileID);
    }

    private void ChangeTileID(EntityTileID entityTileID)
    {
        activeEntityTileID = true;
        this.entityTileID = entityTileID;
    }
    private void ChangeTileID(FloorTileID floorTileID)
    {
        activeEntityTileID = false;
        this.floorTileID = floorTileID;
    }

    private void Update()
    {
        if(disableInput)
            return;
        
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if ((floorTileID == FloorTileID.WATER || floorTileID == FloorTileID.DIRT) && !activeEntityTileID)
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                cachedPosition = hit.point;
                EventSystem.RaiseEvent(EventType.SELECT_TILE_DOWN);
                EventSystem<Vector3, EntityTileID>.RaiseEvent(EventType.SELECT_TILE, hit.point, entityTileID);
            }
            if (Input.GetMouseButton(0) && !IsPointerOverUIElement())
            {
                if (Physics.Raycast(ray, out hit)) {
                    cachedPosition = hit.point;
                    EventSystem<Vector3, EntityTileID>.RaiseEvent(EventType.CHANGE_TILE, cachedPosition, entityTileID);
                }
            }
            return;
        }
        
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUIElement())
        {
            if (Physics.Raycast(ray, out hit)) {
                cachedPosition = hit.point;
                EventSystem.RaiseEvent(EventType.SELECT_TILE_DOWN);
                didPressKeyDown = true;
            }
        }
        if (Input.GetMouseButton(0) && didPressKeyDown)
        {
            if (Physics.Raycast(ray, out hit)) {
                cachedPosition = hit.point;
                EventSystem<Vector3, EntityTileID>.RaiseEvent(EventType.SELECT_TILE, hit.point, entityTileID);
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            didPressKeyDown = false;
            ChangeTile();
            EventSystem.RaiseEvent(EventType.DISABLE_SELECTION_TEXT);
        }
    }

    private void ChangeTile()
    {
        if(activeEntityTileID)
            EventSystem<Vector3, EntityTileID>.RaiseEvent(EventType.CHANGE_TILE, cachedPosition, entityTileID);
        else
            EventSystem<Vector3, FloorTileID>.RaiseEvent(EventType.CHANGE_TILE, cachedPosition, floorTileID);
    }

    private void ToggleInput(bool value)
    {
        disableInput = value;
    }
    
    private bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }


    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == UILayer)
                return true;
        }
        return false;
    }


    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
}

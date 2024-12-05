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
    [SerializeField] private TileID tileID;
    private Vector3 cachedPosition;
    private int UILayer;
    private bool didPressKeyDown = false;

    private void OnEnable()
    {
        UILayer = LayerMask.NameToLayer("UI");
        
        EventSystem<bool>.Subscribe(EventType.TOGGLE_INPUT, ToggleInput);
        EventSystem<TileID>.Subscribe(EventType.CHANGE_BRUSH, ChangeTileID);
    }
    private void OnDisable()
    {
        EventSystem<bool>.Unsubscribe(EventType.TOGGLE_INPUT, ToggleInput);
        EventSystem<TileID>.Unsubscribe(EventType.CHANGE_BRUSH, ChangeTileID);
    }

    private void ChangeTileID(TileID tileID)
    {
        this.tileID = tileID;
    }

    private void Update()
    {
        if(disableInput)
            return;
        
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (tileID == TileID.WATER || tileID == TileID.DIRT)
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Default"))) {
                cachedPosition = hit.point;
                EventSystem<Vector3, TileID>.RaiseEvent(EventType.SELECT_TILE_DOWN, hit.point, tileID);
            }
            if (Input.GetMouseButton(0) && !IsPointerOverUIElement())
            {
                if (Physics.Raycast(ray, out hit)) {
                    cachedPosition = hit.point;
                    EventSystem<Vector3, TileID>.RaiseEvent(EventType.CHANGE_TILE, cachedPosition, tileID);
                }
            }
            return;
        }
        
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUIElement())
        {
            if (Physics.Raycast(ray, out hit)) {
                cachedPosition = hit.point;
                EventSystem<Vector3, TileID>.RaiseEvent(EventType.SELECT_TILE_DOWN, hit.point, tileID);
                didPressKeyDown = true;
            }
        }
        if (Input.GetMouseButton(0) && didPressKeyDown)
        {
            if (Physics.Raycast(ray, out hit)) {
                cachedPosition = hit.point;
                EventSystem<Vector3, TileID>.RaiseEvent(EventType.SELECT_TILE, hit.point, tileID);
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            didPressKeyDown = false;
            EventSystem<Vector3, TileID>.RaiseEvent(EventType.CHANGE_TILE, cachedPosition, tileID);
            EventSystem.RaiseEvent(EventType.DISABLE_SELECTION_TEXT);
        }
    }

    private void ToggleInput(bool value)
    {
        disableInput = value;
    }
    
    public bool IsPointerOverUIElement()
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

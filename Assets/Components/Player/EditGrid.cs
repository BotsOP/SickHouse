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
    [SerializeField] private LayerMask droneLayer;
    [SerializeField] private LayerMask gridLayer;
    [SerializeField] private LayerMask bulldozerLayer;
    private Vector3 cachedPosition;
    private int UILayer;
    private bool didPressKeyDown = false;

    private void OnEnable()
    {
        UILayer = LayerMask.NameToLayer("UI");
        
        EventSystem<bool>.Subscribe(EventType.TOGGLE_INPUT, ToggleInput);
        EventSystem<EntityTileID>.Subscribe(EventType.CHANGE_BRUSH, ChangeTileID);
    }
    private void OnDisable()
    {
        EventSystem<bool>.Unsubscribe(EventType.TOGGLE_INPUT, ToggleInput);
        EventSystem<EntityTileID>.Unsubscribe(EventType.CHANGE_BRUSH, ChangeTileID);
    }

    private void ChangeTileID(EntityTileID entityTileID)
    {
        this.entityTileID = entityTileID;
    }

    private void Update()
    {
        if(disableInput)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            EventSystem.RaiseEvent(EventType.DISABLE_SELECTION_TEXT);
            return;
        }
        
        // return;
        
        if (IsPointerOverUIElement())
        {
            EventSystem.RaiseEvent(EventType.DISABLE_SELECTION_TEXT);
            return;
        }
        
        if (IsInLayerMask(hit.transform, gridLayer))
        {
            cachedPosition = hit.point;
        
            EventSystem.RaiseEvent(EventType.SELECT_TILE_DOWN);
            EventSystem<Vector3, EntityTileID>.RaiseEvent(EventType.SELECT_TILE, hit.point, entityTileID);
        
            if (Input.GetMouseButton(0) && entityTileID is EntityTileID.DIRT or EntityTileID.WATER)
            {
                if (entityTileID == EntityTileID.DIRT)
                {
                    EventSystem<Vector3, EntityTileID[]>.RaiseEvent(EventType.CHANGE_TILE, cachedPosition, new[] { EntityTileID.EMPTY, EntityTileID.DIRT, EntityTileID.EMPTY });
                    return;
                }
            
                EventSystem<Vector3, EntityTileID>.RaiseEvent(EventType.CHANGE_TILE, cachedPosition, entityTileID);
            }
            if (Input.GetMouseButtonDown(0) && entityTileID == EntityTileID.TREE)
            {
                EventSystem<Vector3, EntityTileID>.RaiseEvent(EventType.CHANGE_TILE, cachedPosition, entityTileID);
            }
        }

        if (!Input.GetMouseButtonDown(0))
            return;
        
        if (IsInLayerMask(hit.transform, droneLayer))
        {
            EventSystem<GameObject>.RaiseEvent(EventType.HIT_DRONE, hit.transform.gameObject);
        }
        if (IsInLayerMask(hit.transform, bulldozerLayer))
        {
            EventSystem.RaiseEvent(EventType.HIT_GIANT_BULLDOZER);
        }
    }
    
    
    bool IsInLayerMask(Transform obj, LayerMask mask)
    {
        return (mask.value & (1 << obj.gameObject.layer)) != 0;
    }

    private void ToggleInput(bool value)
    {
        disableInput = value;
    }
    
    private bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }
    
    // Returns 'true' if we touched or hovering on Unity UI element.
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
    
    // Gets all event system raycast results of current mouse or touch position.
     static List<RaycastResult> GetEventSystemRaycastResults()
     {
         PointerEventData eventData = new PointerEventData(UnityEngine.EventSystems.EventSystem.current);
         eventData.position = Input.mousePosition;
         List<RaycastResult> raysastResults = new List<RaycastResult>();
         UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, raysastResults);
         return raysastResults;
     }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EditGrid : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform pointer;
    [SerializeField] private TileID tileID;
    [SerializeField] private List<SelectionBox> selectionBoxes;
    [SerializeField] private List<SelectionSphere> selectionSpheres;
    private Vector3 cachedPosition;

    private void Update()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButton(0))
        {
            if (Physics.Raycast(ray, out hit)) {
                pointer.position = hit.point;
                cachedPosition = hit.point;
                gridManager.PlacementSelection(hit.point, tileID);
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            gridManager.ChangeTile(cachedPosition, tileID);
        }
    }

    public void ChangeTileSelect(TileID tileID)
    {
        this.tileID = tileID;
    }
}

using System;
using UnityEngine;
using UnityEngine.Serialization;

public class EditGrid : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform pointer;
    [SerializeField] private TileID tileID;

    private void Update()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButton(0))
        {
            if (Physics.Raycast(ray, out hit)) {
                pointer.position = hit.point;
                gridManager.ChangeTile(hit.point, tileID);
            }
        }
    }
}

using UnityEngine;
using VInspector;

[RequireComponent(typeof(GridManager))]
public class GridPainter : MonoBehaviour
{
    [SerializeField] private GridObject gridObject;

    [Button]
    public void SaveGrid()
    {
        if (gridObject == null)
        {
            Debug.LogError($"GridObject is empty");
            return;
        }
        GridManager gridManager = GetComponent<GridManager>();
        gridObject.tiles = gridManager.tiles;
    }
}

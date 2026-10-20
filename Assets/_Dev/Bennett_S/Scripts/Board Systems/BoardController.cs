using System;
using UnityEngine;

[RequireComponent(typeof(TileSelectionController))]
public class BoardController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputHandler _input;

    [Header("Selection References")]
    [SerializeField] private Transform _selectionBox;
    
    [Header("Debug")]
    [SerializeField] private bool _enableUnitCoordinateDebug = false;
    
    private TileSelectionController _selectionController;
    private Grid _grid;
    
    public Vector3 SelectedPosition { get; private set;  }

    private void OnEnable()
    {
        //_input.OnPlacement += PlaceUnit;
    }

    private void OnDisable()
    {
        //_input.OnPlacement -= PlaceUnit;
    }
    
    private void Awake()
    {
        _selectionController = GetComponent<TileSelectionController>();
        _grid = GetComponentInChildren<Grid>();
    }

    private void Update()
    {
        SelectedPosition = _selectionController.GetSelectedTilePosition();
        Vector3Int cellPosition = _grid.WorldToCell(SelectedPosition);
        _selectionBox.position = _grid.GetCellCenterWorld(cellPosition);
    }
    
    [Obsolete("Function does not yet implement the creature class (Placeholder method)")]
    public void PlaceUnit(GameObject unit)
    {
        Debug.LogWarning("Function not yet integrated with creature system, please implement in future. Returning.");
        
        //Tile tileInfo = _boardBuilder.GetTileDataFromPosition(SelectedPosition);
        //if (tileInfo.Occupant) return;
        
        //tileInfo.Occupant = Instantiate(unit, _selectionBox.transform.position, Quaternion.identity).gameObject;
    }
}
using System;
using UnityEngine;

[RequireComponent(typeof(TileSelectionController), typeof(Board))]
public class BoardController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputHandler _input;

    [Header("Selection References")]
    [SerializeField] private Transform _selectionBox;
    
    [Header("Debug")]
    [SerializeField] private bool _enableCreatureCoordinateDebug = false;
    
    private TileSelectionController _selectionController;
    private Board _board;
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
        _board = GetComponent<Board>();
        _grid = GetComponentInChildren<Grid>();
    }

    private void Update()
    {
        SelectedPosition = _selectionController.GetSelectedTilePosition();
        Vector3Int cellPosition = _grid.WorldToCell(SelectedPosition);
        _selectionBox.position = _grid.GetCellCenterWorld(cellPosition);
    }
    
    [Obsolete("Function not yet implemented fully")]
    public void PlaceCreature(Creature creature)
    {
        Tile tile = _board.GetTileFromPosition(SelectedPosition);
        if (tile.Occupant) return;
        
        //tile.Occupant = Instantiate(unit, _selectionBox.transform.position, Quaternion.identity).gameObject;
        // if (_enableCreatureCoordinateDebug)
        // {
        //     Debug.Log($"Placed Creature: {creature.name} at {SelectedPosition}");
        // }
    }

    [Obsolete("Function not yet implemented fully")]
    public void MoveCreature(Creature creature, Vector3 position)
    {
        Tile tile = _board.GetTileFromPosition(position);
        if (tile.Occupant) return;
        
        // if (_enableCreatureCoordinateDebug)
        // {
        //     Debug.Log(position);
        // }
        // Movement logic
    }
}
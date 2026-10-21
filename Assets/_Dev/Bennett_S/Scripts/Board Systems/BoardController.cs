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

    private CreatureData SelectedCreatureData { get; set; }
    
    private TileSelectionController _selectionController;
    private Board _board;
    private Grid _grid;
    
    public Vector3 SelectedPosition { get; private set;  }

    private void OnEnable()
    {
        _input.OnPlacement += PlaceCreature;
    }

    private void OnDisable()
    {
        _input.OnPlacement -= PlaceCreature;
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

    public void PlaceCreature()
    {
        PlaceCreature(SelectedCreatureData);
    }
    
    public Creature PlaceCreature(CreatureData creatureData)
    {
        Tile tile = _board.GetTileFromPosition(SelectedPosition);
        if (tile.Occupant || !tile.Data.IsPassable)
        {
            Debug.Log("Position is Occupied or Impassable, returning");
            return null;
        }

        GameObject creatureObj = Instantiate(creatureData.CreaturePrefab, _selectionBox.transform.position, Quaternion.identity);
        if (!creatureObj.TryGetComponent(out Creature creature))
        {
            Debug.LogError($"Prefab '{creatureObj.name}' does not have a creature component.");
            return null;
        }
        tile.Occupant = creature;
        
        if (_enableCreatureCoordinateDebug) Debug.Log($"Placed Creature: {creatureObj.name} at {SelectedPosition}");
        
        return creature;
    }

    [Obsolete("Function not yet implemented fully")]
    public void MoveCreature(Creature creature, Vector3 position)
    {
        Tile tile = _board.GetTileFromPosition(position);
        if (tile.Occupant || !tile.Data.IsPassable) return;
        
        // Debug logic
        if (_enableCreatureCoordinateDebug)
        {
            Debug.Log(position);
        }
        // Movement logic
    }
}
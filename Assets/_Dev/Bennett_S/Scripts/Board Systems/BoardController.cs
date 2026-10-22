using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TileSelectionController), typeof(Board))]
public class BoardController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputHandler _input;
    
    [Header("Selection Settings")]
    [SerializeField] private Transform _selectionBox;
    [SerializeField] private float _selectionBoxHoverHeight = 0.5f; 
    
    [Header("Debug")]
    [SerializeField] private bool _enableCreatureCoordinateDebug = false;

    [SerializeField] public CreatureData TestingSpawner;
    
    private GameControls _inputMapping;
    private TileSelectionController _selectionController;
    private Board _board;
    private Grid _grid;

    [SerializeField] private Tile _selectedTile;

    public Vector3 SelectedPosition => _selectionBox.position + new Vector3(0, -1, 0); // Offset the position to match tiles

    private void OnEnable()
    {
        _inputMapping.Enable();
        _inputMapping.Gameplay.OnSelect.performed += OnSelectionPressed;
        _inputMapping.Dev.SpawnUnit.performed += PlaceCreature;
    }

    private void OnDisable()
    {
        _inputMapping.Dev.SpawnUnit.performed -= PlaceCreature;
        _inputMapping.Gameplay.OnSelect.performed -= OnSelectionPressed;
        _inputMapping.Disable();
    }
    
    private void Awake()
    {
        _selectionController = GetComponent<TileSelectionController>();
        _board = GetComponent<Board>();
        _grid = GetComponentInChildren<Grid>();
        _inputMapping = new GameControls();
    }

    private void Update()
    {
        HandleTileSelection();
    }

    private void HandleTileSelection()
    {
        if (_selectionController.TryGetSelectedWorldPosition(out Vector3 mouseWorldPosition))
        {
            Tile tile = _board.GetTileFromPosition(mouseWorldPosition);

            if (tile && _board.IsIndexInBounds(tile.TileIndex))
            {
                if (!_selectionBox.gameObject.activeSelf) _selectionBox.gameObject.SetActive(true);
                
                Vector3 targetPosition = tile.transform.position;
                targetPosition.y += _selectionBoxHoverHeight;
                
                _selectionBox.position = targetPosition;
                return;
            }
        }
        
        // Disable selection if outside the board
        if (_selectionBox.gameObject.activeSelf)
        {
            _selectionBox.gameObject.SetActive(false);
        }
    }
    
    public void OnSelectionPressed(InputAction.CallbackContext context)
    {
        if (!_selectedTile)
        {
            SelectCreature();
        }
        else
        {
            MoveCreature(_selectedTile.Occupant, _board.GetTileFromPosition(SelectedPosition));
            _selectedTile = null;
        }
    }

    public void PlaceCreature(InputAction.CallbackContext context)
    {
        PlaceCreature(TestingSpawner);
    }
    
    public Creature PlaceCreature(CreatureData creatureData)
    {
        Tile tile = _board.GetTileFromPosition(SelectedPosition);
        if (tile.Occupant || !tile.Data.IsPassable)
        {
            Debug.Log("Position is Occupied or Impassable, returning");
            return null;
        }

        GameObject creatureObj = Instantiate(creatureData.CreaturePrefab, _selectionBox.position, Quaternion.identity);
        if (!creatureObj.TryGetComponent(out Creature creature))
        {
            Debug.LogError($"Prefab '{creatureObj.name}' does not have a creature component.");
            return null;
        }
        tile.Occupant = creature;
        creature.OccupyingTile = tile;

        if (_enableCreatureCoordinateDebug) Debug.Log($"Placed Creature: {creatureObj.name} at {SelectedPosition}");
        
        return creature;
    }

    [Obsolete("Function not yet implemented fully")]
    public void MoveCreature(Creature creature, Tile target)
    {
        if (target.Occupant || !target.Data.IsPassable) return;
        
        Vector2Int[] validMoves = creature.ShowValidMovement();
        // if there are no valid moves, return
        if (validMoves.Length == 0) return;

        Vector2Int relativeTargetPoistion = target.TileIndex - creature.OccupyingTile.TileIndex;
        // if the target tile is not in the valid moves, return
        if (!Array.Exists(validMoves, move => move == relativeTargetPoistion)) return;

        creature.OccupyingTile.Occupant = null;
        target.Occupant = creature;
        creature.OccupyingTile = target;
        creature.transform.position = target.transform.position;
        creature.transform.position += new Vector3(0, creature.OccupyingTile.Data.CreatureYOffset, 0);

        // Debug logic
        if (_enableCreatureCoordinateDebug)
        {
            Debug.Log(target.gameObject.transform.position);
        }
    }

    [Obsolete("Function not yet implemented fully")]
    public void PreviewPattern(Tile tile, bool isAttack)
    {
        if (!tile.Occupant) return;

        Vector2Int[] validMoves = isAttack ? tile.Occupant.ShowValidAttacks() : tile.Occupant.ShowValidMovement();

        //Render Pattern on the board
    }

    public void SelectCreature()
    {
        Tile tile = _board.GetTileFromPosition(SelectedPosition);
        if (!tile.Occupant) return;
        _selectedTile = tile;
    }
}
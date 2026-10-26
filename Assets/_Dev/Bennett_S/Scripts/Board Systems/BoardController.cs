using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TileSelectionController), 
    typeof(TileHighlightingController), 
    typeof(Board))]
public class BoardController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputHandler _input;
    [SerializeField] private MatchController _matchController;
    
    [Header("Selection Settings")]
    [SerializeField] private Transform _selectionBox;
    [SerializeField] private float _selectionBoxHoverHeight = 0.5f; 
    
    [Header("Debug")]
    [SerializeField] private bool _enableCreatureCoordinateDebug = false;
    
    private GameControls _inputMapping;
    private TileSelectionController _selectionController;
    private TileHighlightingController _highlightingController;
    private Board _board;

    [SerializeField] private Tile _selectedTile;

    private CreatureStage _creatureStage = CreatureStage.None;
        
    public Vector3 SelectedPosition => _selectionBox.position + new Vector3(0, -1, 0); // Offset the position to match tiles

    public static event Action<Creature> OnCreatureSelected;
    
    private void OnEnable()
    {
        _inputMapping.Enable();
        _inputMapping.Gameplay.OnSelect.performed += OnSelectionPressed;
        _inputMapping.Dev.SpawnUnit.performed += PlaceCreature;
        _inputMapping.Gameplay.ViewAttack.performed += OnViewAttack;
        _inputMapping.Gameplay.ViewMove.performed += OnViewMove;

        MatchController.OnTurnEnded += OnTurnEnded;
    }

    private void OnDisable()
    {
        _inputMapping.Dev.SpawnUnit.performed -= PlaceCreature;
        _inputMapping.Gameplay.OnSelect.performed -= OnSelectionPressed;
        _inputMapping.Gameplay.ViewAttack.performed -= OnViewAttack;
        _inputMapping.Gameplay.ViewMove.performed -= OnViewMove;
        _inputMapping.Disable();
        
        MatchController.OnTurnEnded -= OnTurnEnded;
    }

    private void Awake()
    {
        _selectionController = GetComponent<TileSelectionController>();
        _highlightingController = GetComponent<TileHighlightingController>();
        _board = GetComponent<Board>();
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
    
    private void PlaceCreature(InputAction.CallbackContext context)
    {
        if (!_matchController || !_matchController.IsMatchActive) return;
        
        if (_matchController.CurrentState != MatchState.Summon)
        {
            Debug.Log("BoardController: Creatures cannot be summoned outside of the Summon phase");
            return;
        }
        
        IPlayer activePlayer = _matchController.CurrentPlayer;
        if (activePlayer == null) return;
        
        CreatureData creatureData = activePlayer.SelectedCreature;
        if (creatureData == null)
        {
            Debug.LogWarning($"BoardController: {activePlayer.Data.Name} tried to spawn a creature, but hasn't selected a creature yet.");
            return;
        }
        
        PlaceCreature(creatureData);
    }
    
    public Creature PlaceCreature(CreatureData creatureData)
    {
        IPlayer activePlayer = _matchController.CurrentPlayer;
        if (activePlayer == null) return null;
        
        Tile tile = _board.GetTileFromPosition(SelectedPosition);
        if (tile.Occupant || !tile.Data.IsPassable)
        {
            Debug.Log("Position is Occupied or Impassable, returning");
            return null;
        }

        GameObject creatureObj = Instantiate(creatureData.CreaturePrefab, tile.transform.position, Quaternion.identity);
        creatureObj.transform.position += new Vector3(0, tile.Data.CreatureYOffset, 0);
        
        if (!creatureObj.TryGetComponent(out Creature creature))
        {
            Debug.LogError($"Prefab '{creatureObj.name}' does not have a creature component.");
            Destroy(creatureObj);
            return null;
        }
        
        creature.Data = creatureData;
        creature.Owner = activePlayer.Data;
        creature.CurrentHealth = creatureData.MaxHealth;
        creature.Experience = 0;
        creature.OccupyingTile = tile;
        
        tile.Occupant = creature;
        
        activePlayer.AddCreature(creature);
        creatureObj.name = creature.Data.CreatureName;
        
        if (_enableCreatureCoordinateDebug) Debug.Log($"Placed Creature: {creatureObj.name} at {SelectedPosition}");
        
        return creature;
    }
    
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
    
    public void PreviewPattern(Tile tile, bool isAttack)
    {
        if (!tile.Occupant) return;

        Vector2Int[] validMoves = isAttack ? tile.Occupant.ShowValidAttacks() : tile.Occupant.ShowValidMovement();
        List<Vector3> validWorldPositions = new();

        // Convert 2D positions into 3D (including tile height offset)
        foreach (Vector2Int position in validMoves)
        {
            // Convert relative to creature coordinate to relative to board coordinate
            Vector2Int relativePosition = position + tile.TileIndex;
            
            // Check if tile is impassable
            if (!tile.Data.IsPassable) continue;
            // Check if world position can be retrieved from the given index
            if (!_board.TryGetWorldPositionFromIndex(relativePosition, out Vector3 worldPosition)) continue;

            // Not complete, ask Ben for details
            if (isAttack)
            {
                // Show attack over occupied tiles
            }
            else
            {
                // Don't show movement highlights over inaccessible tiles
                Tile tileToCheck = _board.GetTileFromPosition(worldPosition);
                if (!tileToCheck.Data.IsPassable || tileToCheck.Occupant) continue;
            }
            //
            
            validWorldPositions.Add(worldPosition);
        }
        
        // Add highlight to selected creature's tile
        validWorldPositions.Add(tile.transform.position);
        
        _highlightingController.HighlightTiles(validWorldPositions, isAttack);
        
        // Add a check for impassable tiles and cut off the movement
    }

    private void SelectCreature()
    {
        Tile tile = _board.GetTileFromPosition(SelectedPosition);
        if (!tile.Occupant) return;
        
        // Return for creatures selected other than the player's own
        if (tile.Occupant.Owner.Name != _matchController.CurrentPlayer.Data.Name) return;
        
        _selectedTile = tile;
        OnCreatureSelected?.Invoke(tile.Occupant);
        
        _creatureStage = CreatureStage.Moving;
        PreviewPattern(_selectedTile, false);
        
        if (_enableCreatureCoordinateDebug) Debug.Log($"Selected Creature: {tile.Occupant.Data.name} at {tile.TileIndex}");
    }
    
    private void OnSelectionPressed(InputAction.CallbackContext context)
    {
        if (!_matchController || !_matchController.IsMatchActive) return;
        
        if (!_selectedTile)
        {
            SelectCreature();
        }
        else
        {
            if (_creatureStage != CreatureStage.Moving) return;
            
            MoveCreature(_selectedTile.Occupant, _board.GetTileFromPosition(SelectedPosition));
            _selectedTile = null;
            
            _creatureStage = CreatureStage.None;
            _highlightingController.Clear();
        }
    }
    
    private void OnViewAttack(InputAction.CallbackContext context)
    {
        _creatureStage = CreatureStage.Attacking;
        _highlightingController.Clear();
        PreviewPattern(_selectedTile, true);
    }

    private void OnViewMove(InputAction.CallbackContext context)
    {
        _creatureStage = CreatureStage.Moving;
        _highlightingController.Clear();
        PreviewPattern(_selectedTile, false);
    }
    
    private void OnTurnEnded(IPlayer player)
    {
        _selectedTile = null;
        _highlightingController.Clear();
    }
}

public enum CreatureStage
{
    None,
    Moving,
    Attacking,
}
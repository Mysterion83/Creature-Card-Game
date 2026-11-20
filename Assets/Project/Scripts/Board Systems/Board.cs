using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private GameObject _tilePrefab;

    [SerializeField, Space] private Transform _tileContainer;
    [SerializeField] private Transform _shaderPlane;
    [SerializeField] private Material _gridShaderMaterial;

    [Header("Board Generation")]
    [SerializeField] private SavedMapData _definitionData;
    [SerializeField] private Vector3 _tileSpawnOffset = new Vector3(0, 0.5f, 0);

    [Header("Active Zones")]
    [SerializeField] public List<ZoneInstance> ActiveZones = new();
    
    [Header("Shader Settings")] 
    [SerializeField] private float _gridYOffset = 0.015f;
    [SerializeField, Range(0.01f, 0.1f)] private float _cellThickness = 0.1f;
    [SerializeField] private Color _gridColour = Color.white;
    
    [Header("Debugging")]
    [SerializeField] private bool _enableZoneDebugging;
    
    private static readonly int Thickness = Shader.PropertyToID("_Thickness");
    private static readonly int Colour = Shader.PropertyToID("_Colour");
    
    private Grid _grid;
    private MapData _mapData;

    private void Awake()
    {
        if (!_definitionData)
        {
            Debug.LogError(
                "Board Loading Error: SavedMapData unassigned, please make sure it is assigned within the inspector.");
            return;
        }

        if (!_tilePrefab)
        {
            Debug.LogError(
                "Board Loading Error: Tile prefab unassigned, please make sure it is assigned within the inspector.");
            return;
        }

        _grid = GetComponentInChildren<Grid>();

        // Load map data and build the board
        _mapData = LoadMapData(_definitionData);

        InitialiseZones();
    }

    private void Start()
    {
        float gridXOffset = (_mapData.MapSize.x % 2 != 0) ? 0.5f : 0f;
        float gridZOffset = (_mapData.MapSize.y % 2 != 0) ? 0.5f : 0f;

        // Offset the Grid so WorldToCell and GetCellCenterWorld line up
        _grid.transform.position = new Vector3(gridXOffset, 0, gridZOffset);

        // Apply Shader Settings
        _shaderPlane.localScale = new Vector3(_mapData.MapSize.x * 0.1f, 1, _mapData.MapSize.y * 0.1f);
        _shaderPlane.position = new Vector3(0, _shaderPlane.position.y + _gridYOffset, 0);

        _gridShaderMaterial.SetFloat(Thickness, _cellThickness);
        _gridShaderMaterial.SetColor(Colour, _gridColour);
    }

    public MapData LoadMapData(SavedMapData data)
    {
        if (data == null || data.MapSize.x <= 0 || data.MapSize.y <= 0)
        {
            Debug.LogError("Runtime Loading Error: Invalid SavedMapData asset provided.");
            return default;
        }

        if (!_tilePrefab || !_tilePrefab.GetComponent<Tile>())
        {
            Debug.LogError("Runtime Loading Error: Tile prefab is missing or does not contain a Tile component!");
            return default;
        }

        MapData mapData = new MapData
        {
            MapSize = data.MapSize,
            Tiles = new Tile[data.MapSize.x, data.MapSize.y]
        };

        Vector3Int bottomLeftTileOffset = new Vector3Int(-(data.MapSize.x / 2), -(data.MapSize.y / 2), 0);

        for (int x = 0; x < data.MapSize.x; x++)
        {
            for (int y = 0; y < data.MapSize.y; y++)
            {
                // Obtain flat index (2D to 1D translation)
                int flatIndex = x + (y * data.MapSize.x);

                if (flatIndex >= data.Tiles.Length) continue;

                SavedTileData savedTileData = data.Tiles[flatIndex];
                Vector3 spawnPosition = _grid.CellToWorld(new Vector3Int(x, y, 0) + bottomLeftTileOffset);

                // Apply position offsets for even boards
                if (data.MapSize.x % 2 == 0) spawnPosition.x += 0.5f;
                if (data.MapSize.y % 2 == 0) spawnPosition.z += 0.5f;
                spawnPosition.y = -0.5f;

                // Create Tile
                GameObject tileInstance = Instantiate(
                    _tilePrefab,
                    spawnPosition + _tileSpawnOffset, 
                    savedTileData.Rotation, 
                    _tileContainer);
                Tile tile = tileInstance.GetComponent<Tile>();

                tile.Data = TileRegistry.Instance.GetTileData(savedTileData.TileID);
                tile.TileIndex = new Vector2Int(x, y);
                
                tile.AssignedZone = savedTileData.ZoneDefinition;
                tile.IsZoneCenter = savedTileData.IsZoneCenter;

                tile.IsSpawnTile = savedTileData.IsSpawnTile;
                tile.SpawnOwnerID = savedTileData.SpawnOwnerID;
                
                tile.Init();

                mapData.Tiles[x, y] = tile;
            }
        }

        return mapData;
    }

    public MapData GetData() => _mapData;
    
    #region Zoning
    /// <summary>
    /// Validates that every registered player has at least one valid spawn tile assigned on the board.
    /// </summary>
    public bool ValidateSpawnZones()
    {
        if (_mapData.Tiles == null) return false;

        int playerCount = PlayerRegistry.PlayerCount;
        if (playerCount == 0)
        {
            Debug.LogWarning("Board Warning: No players registered yet during spawn validation.");
            return true; 
        }

        // Keep track of which players have a spawn tile
        HashSet<int> foundSpawnOwners = new();

        foreach (Tile tile in _mapData.Tiles)
        {
            if (tile && tile.IsSpawnTile)
            {
                foundSpawnOwners.Add(tile.SpawnOwnerID);
            }
        }

        // Verify every player has a spawn zone
        bool isValid = true;
        for (int i = 0; i < playerCount; i++)
        {
            if (foundSpawnOwners.Contains(i)) continue;
            
            string playerName = PlayerRegistry.MasterList[i]?.Data?.Name ?? $"Player {i}";
            Debug.LogError($"Board Error: Player index {i} ({playerName}) has no designated spawn tiles assigned on this map!");
            isValid = false;
        }

        return isValid;
    }
    
    /// <summary>
    /// Initialise all the board's zones and construct their patterns
    /// </summary>
    private void InitialiseZones()
    {
        if (_mapData.Tiles == null) return;
        ActiveZones.Clear();
        int zoneCounter = 0;

        foreach (Tile tile in _mapData.Tiles)
        {
            if (!tile || !tile.IsZoneCenter || !tile.AssignedZone) continue;

            // Create zone instance and add the central tile
            ZoneInstance newZone = new ZoneInstance(zoneCounter++, tile.AssignedZone, tile);
            newZone.RelativeTiles.Add(tile);
            
            tile.ZoneID = newZone.ZoneID; 
            
            // Loop through zone offsets from the ZoneDefinition and assign them
            foreach (Vector2Int offset in tile.AssignedZone.RelativeOffsets)
            {
                Vector2Int targetCoords = tile.TileIndex + offset;
                if (!IsIndexInBounds(targetCoords)) continue;

                Tile affectedTile = _mapData.Tiles[targetCoords.x, targetCoords.y];
                
                // Only claim if ID is below 0
                if (!affectedTile || affectedTile.ZoneID >= 0) continue;
                
                // Set ZoneID to the initialised Zone
                affectedTile.ZoneID = newZone.ZoneID;
                
                //Prevent duplicate tile entries
                if (!newZone.RelativeTiles.Contains(affectedTile))
                {
                    newZone.RelativeTiles.Add(affectedTile);
                }
            }

            ActiveZones.Add(newZone);
        }
    }
    
    /// <summary>
    /// Evaluate all zones to see if a player controls them
    /// </summary>
    public void EvaluateZoneControl()
    {
        foreach (var zone in ActiveZones)
        {
            Dictionary<Player, int> playerCreatureCounts = new Dictionary<Player, int>();

            // Retrieve every player
            foreach (Tile tile in zone.RelativeTiles)
            {
                if (!tile.Occupant) continue;
                
                Player owner = tile.Occupant.Owner;
                playerCreatureCounts.TryAdd(owner, 0);
                playerCreatureCounts[owner]++;
            }

            float bonusEnergy = zone.Definition.BonusEnergy;
            Player controller = DetermineMajorityOwner(playerCreatureCounts);
            
            // Continue if occupier is the same
            if (zone.Occupier == controller) continue; 

            // Remove bonus energy from previous Occupier (if they exist)
            if (zone.Occupier)
            {
                if (_enableZoneDebugging) Debug.Log($"[Board] {zone.Occupier.Data.Name} lost control of '{zone.Definition.ZoneName}'.");
                zone.Occupier.EnergySystem.RemoveBonusEnergy(bonusEnergy);
            }
            
            zone.SetOccupier(controller);

            // Check for a winner (not a tie/vacant) and add bonus energy
            if (controller)
            {
                if (_enableZoneDebugging) Debug.Log($"[Board] New Zone Occupier of '{zone.Definition.ZoneName}': {controller.Data.Name}");
                controller.EnergySystem.AddBonusEnergy(bonusEnergy);
            }
            else
            {
                if (_enableZoneDebugging) Debug.Log($"[Board] '{zone.Definition.ZoneName}' is now contested or empty.");
            }
        }
    }
    
    /// <summary>
    /// Evaluate who the majority owner of a zone is
    /// </summary>
    /// <param name="counts">Dictionary of [Player, Creature Count]s to check</param>
    /// <returns>The <c>Player</c> that controls the zone, null if majority is shared</returns>
    private Player DetermineMajorityOwner(Dictionary<Player, int> counts)
    {
        Player leader = null;
        int maxCount = 0;
        bool isTie = false;

        foreach (var pair in counts)
        {
            if (pair.Value > maxCount)
            {
                maxCount = pair.Value;
                leader = pair.Key;
                isTie = false;
            }
            else if (pair.Value == maxCount)
            {
                isTie = true;
            }
        }

        return isTie ? null : leader;
    }
    #endregion
    
    #region Tile Helper Functions
    /// <summary>
    /// Get a <c>Tile</c> through a given world position
    /// </summary>
    /// <param name="worldPosition">Vector3 position to retrieve data from</param>
    /// <returns>Tile if position is valid, null otherwise</returns>
    public Tile GetTileFromPosition(Vector3 worldPosition) // May need to rework this in future for vertical maps to overlap capsule
    {
        if (_mapData.Tiles == null) return null;

        Collider[] colliders = Physics.OverlapSphere(worldPosition, 0.1f);

        foreach (var col in colliders)
        {
            if (col.TryGetComponent(out Tile tile))
            {
                return _mapData.Tiles[tile.TileIndex.x, tile.TileIndex.y];
            }
        }

        return null;
    }

    /// <summary>
    /// Attempt to get the 3D world position of a <c>Tile</c> using its grid index coordinates
    /// </summary>
    /// <param name="gridIndex">The grid index for a tile within the board space</param>
    /// <param name="worldPosition">The found Vector3 position, defaults to Vector3.zero</param>
    /// <returns>true if position is valid, false if not</returns>
    public bool TryGetWorldPositionFromIndex(Vector2Int gridIndex, out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;
        if (_mapData.Tiles == null || !IsIndexInBounds(gridIndex)) return false;

        Tile tile = _mapData.Tiles[gridIndex.x, gridIndex.y];
        if (tile == null) return false;
        
        worldPosition = tile.transform.position;
        return true;
    }
    
    /// <summary>
    /// Get a <c>Tile</c>'s definition data through a given world position
    /// </summary>
    /// <param name="worldPosition">Vector3 position to retrieve data from</param>
    /// <returns><c>TileData</c> if position is valid, null otherwise</returns>
    public TileData GetTileDataFromPosition(Vector3 worldPosition)
    {
        Tile tile = GetTileFromPosition(worldPosition);
        return tile == null ? null : tile.Data;
    }

    /// <summary>
    /// Checks to see if a given index is valid within the space of the board
    /// </summary>
    /// <param name="index">The index to check</param>
    /// <returns>true if found, null if not found or if the tiles within the map are null</returns>
    public bool IsIndexInBounds(Vector2Int index)
    {
        if (_mapData.Tiles == null) return false;
        return index.x >= 0 && index.x < _mapData.MapSize.x && 
               index.y >= 0 && index.y < _mapData.MapSize.y;
    }
    #endregion
}
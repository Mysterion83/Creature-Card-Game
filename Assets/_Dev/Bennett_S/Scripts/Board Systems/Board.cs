using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private GameObject _tilePrefab;

    [SerializeField, Space] private Transform _tileContainer;
    [SerializeField] private Transform _shaderPlane;
    [SerializeField] private Material _gridShaderMaterial;

    [Header("Map Data to Load")] [SerializeField]
    private SavedMapData _definitionData;

    [Header("Generation Settings")] [SerializeField]
    private Vector3 _tileSpawnOffset = new Vector3(0, 0.5f, 0);

    [Header("Shader Settings")] [SerializeField]
    private float _gridYOffset = 0.015f;

    [SerializeField, Range(0.01f, 0.1f)] private float _cellThickness = 0.1f;
    [SerializeField] private Color _gridColour = Color.white;

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

        _gridShaderMaterial.SetFloat("_Thickness", _cellThickness);
        _gridShaderMaterial.SetColor("_Colour", _gridColour);
    }

    public MapData LoadMapData(SavedMapData data)
    {
        if (data == null || data.MapSize.x <= 0 || data.MapSize.y <= 0)
        {
            Debug.LogError("Runtime Loading Error: Invalid SavedMapData asset provided.");
            return default(MapData);
        }

        if (!_tilePrefab || !_tilePrefab.GetComponent<Tile>())
        {
            Debug.LogError("Runtime Loading Error: Tile prefab is missing or does not contain a Tile component!");
            return default(MapData);
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
                GameObject tileInstance = Instantiate(_tilePrefab, spawnPosition + _tileSpawnOffset,
                    Quaternion.identity, _tileContainer);
                Tile tile = tileInstance.GetComponent<Tile>();

                tile.Data = TileRegistryV2.Instance.GetTileData(savedTileData.TileID);
                tile.TileIndex = new Vector2Int(x, y);
                tile.RegionID = savedTileData.Region;

                tile.Init();

                mapData.Tiles[x, y] = tile;
            }
        }

        return mapData;
    }

    public MapData GetData() => _mapData;

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
            if (col.TryGetComponent<Tile>(out Tile tile))
            {
                // Instantly pull the exact array slot without any division or truncation math
                return _mapData.Tiles[tile.TileIndex.x, tile.TileIndex.y];
            }
        }

        return null;
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
}
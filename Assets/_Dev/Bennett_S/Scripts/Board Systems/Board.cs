using System;
using UnityEngine;

[RequireComponent(typeof(BoardController))]
public class Board : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _tilePrefab;

    [SerializeField, Space] private Transform _tileContainer;
    [SerializeField] private Transform _shaderPlane;
    
    [Header("Map Data to Load")]
    [SerializeField] private SavedMapData _definitionData;
    
    [Header("Settings")]
    [SerializeField] private float _gridYOffset = 0.015f;
    
    private Grid _grid;
    private MapData _mapData;

    private void Awake()
    {
        if (!_definitionData)
        {
            Debug.LogError("Board Loading Error: SavedMapData unassigned, please make sure it is assigned within the inspector.");
            return;
        }
        if (!_tilePrefab)
        {
            Debug.LogError("Board Loading Error: Tile prefab unassigned, please make sure it is assigned within the inspector.");
            return;
        }
     
        _grid = GetComponentInChildren<Grid>();
    }

    private void Start()
    {
        // Load map data and build the board
        _mapData = LoadMapData(_definitionData);
        
        float gridXOffset = (_mapData.MapSize.x % 2 != 0) ? 0.5f : 0f;
        float gridZOffset = (_mapData.MapSize.y % 2 != 0) ? 0.5f : 0f;
    
        // Offset the Grid so WorldToCell and GetCellCenterWorld line up
        _grid.transform.position = new Vector3(gridXOffset, 0, gridZOffset);
        
        // Apply Shaders
        _shaderPlane.localScale = new Vector3(_mapData.MapSize.x * 0.1f, 1, _mapData.MapSize.y * 0.1f);
        _shaderPlane.position = new Vector3(0, _shaderPlane.position.y + _gridYOffset, 0);
    }

    public MapData LoadMapData(SavedMapData data)
    {
        MapData mapData = new MapData
        {
            MapSize = data.MapSize,
            Tiles = new Tile[data.MapSize.x, data.MapSize.y]
        };
        
        Vector3 gridWorldOrigin = _grid.CellToWorld(Vector3Int.zero);

        for (int currentTile = 0; currentTile < data.Tiles.Length; currentTile++)
        {
            int currentCol = currentTile % mapData.MapSize.x;
            int currentRow = currentTile / mapData.MapSize.x;
            
            Vector3 centerOffset = new Vector3(data.MapSize.x - 1, 0, data.MapSize.y - 1) * 0.5f;
            Vector3 localPosition = new Vector3(currentCol, -0.5f, currentRow) - centerOffset;
            Vector3 spawnPosition = gridWorldOrigin + localPosition;

            GameObject tileInstance = Instantiate(_tilePrefab, 
                spawnPosition,  
                Quaternion.identity,
                _tileContainer);
            
            Tile tile = tileInstance.GetComponent<Tile>();
        
            if (!tile)
            {
                Debug.LogError("Board Loading Error: Tile prefab does not contain a Tile component! Please make sure the correct prefab is assigned within the inspector.");
                break;
            }
            
            tile.RegionID = data.Tiles[currentTile].Region;
            tile.Data = TileRegistry.Instance.GetTileData(data.Tiles[currentTile].TileID);
        
            tile.Init();
            
            mapData.Tiles[currentCol, currentRow] = tile;
        }
        return mapData;
    }

    public MapData GetData() => _mapData;
}
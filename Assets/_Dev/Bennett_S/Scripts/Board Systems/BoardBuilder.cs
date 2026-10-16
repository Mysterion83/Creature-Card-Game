using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public class BoardBuilder : MonoBehaviour
{
    [Header("Board References")]
    [SerializeField] private Grid _grid;
    [SerializeField] private Transform _shaderPlane;
    
    [Header("Build Settings")]
    [SerializeField] private MapData _mapData; // Struct type
    [SerializeField] private string _fileName = "New MapSaveData";

    [Header("Debug")]
    [SerializeField] private bool _showTileDataCreation = false;
    
    [Header("Test Tile")]
    [SerializeField] private GameObject _defaultTilePrefab;
    
    private GameObject _prevTileContainer;
    private GameObject _tileContainer;
    private Vector3Int _bottomLeftTileOffset;
    private Vector2Int _previousGridSize;
     
    private void Resize()
    {
        _previousGridSize = _mapData.MapSize;
        _bottomLeftTileOffset = new Vector3Int(-(_mapData.MapSize.x / 2), -(_mapData.MapSize.y / 2), 0);
                
        EditorApplication.delayCall += () =>
        {
            if (_prevTileContainer) DestroyImmediate(_prevTileContainer);
        };

        if (!_shaderPlane) return;
        _shaderPlane.localScale = new Vector3(_mapData.MapSize.x * 0.1f, 1, _mapData.MapSize.y * 0.1f);
        _shaderPlane.position = new Vector3(0, 0.015f, 0);
    }

    public void LoadFromAsset()
    {
        string filePath = EditorUtility.OpenFilePanel("Load Saved Map Data", Application.dataPath, "asset");
        if (string.IsNullOrEmpty(filePath)) return;

        filePath = $"Assets{filePath.Substring(Application.dataPath.Length)}";
        SavedMapData mapToLoad;
        try
        {
            mapToLoad = (SavedMapData)AssetDatabase.LoadAssetAtPath(filePath, typeof(SavedMapData));
        }
        catch (NullReferenceException)
        {
            Debug.LogError($"BoardBuilder Load Error: Couldn't load asset at path {filePath}, object type is not SavedMapData");
            return;
        }

        CustomBuild(mapToLoad);
    }
    
    public void DestroyTiles()
    {
        EditorApplication.delayCall += () =>
        {
            if (_tileContainer) DestroyImmediate(_tileContainer);
        };
    }
    
    public void Build()
    {
        Resize();
        
        _mapData.Tiles = new Tile[_mapData.MapSize.x, _mapData.MapSize.y];

        _prevTileContainer = _tileContainer;
        _tileContainer = new GameObject("Tile Container");
        _tileContainer.transform.SetParent(transform);
        
        if (_prevTileContainer) Undo.DestroyObjectImmediate(_prevTileContainer);
        Undo.RegisterCreatedObjectUndo(_tileContainer, "Instantiate Tile Container");
        
        for (int x = 0; x < _mapData.MapSize.x; x++)
        {
            for (int y = 0; y < _mapData.MapSize.y; y++)
            {
                Vector3 spawnPosition = CalculateSpawnPosition(x, y);
                CreateEditorTile(x, y, spawnPosition, null); // Null as no save data being added
            }
        }
    }

    private void CustomBuild(SavedMapData mapToLoad)
    {
        _mapData.MapSize = mapToLoad.MapSize;
        
        Resize();

        _mapData.Tiles = new Tile[_mapData.MapSize.x, _mapData.MapSize.y];

        _prevTileContainer = _tileContainer;
        _tileContainer = new GameObject("Tile Container");
        _tileContainer.transform.SetParent(transform);
        
        if (_prevTileContainer) Undo.DestroyObjectImmediate(_prevTileContainer);
        Undo.RegisterCreatedObjectUndo(_tileContainer, "Instantiate Tile Container");
        
        for (int x = 0; x < _mapData.MapSize.x; x++)
        {
            for (int y = 0; y < _mapData.MapSize.y; y++)
            {
                int flatIndex = x + (y * mapToLoad.MapSize.x);
                
                if (flatIndex >= mapToLoad.Tiles.Length) continue;

                Vector3 spawnPosition = CalculateSpawnPosition(x, y);
                CreateEditorTile(x, y, spawnPosition, mapToLoad.Tiles[flatIndex]);
            }
        }
    }

    private Vector3 CalculateSpawnPosition(int x, int y)
    {
        Vector3 spawnPosition = _grid.CellToWorld(new Vector3Int(x, y, 0) + _bottomLeftTileOffset);
        
        if (_mapData.MapSize.x % 2 == 0) spawnPosition.x += 0.5f;
        if (_mapData.MapSize.y % 2 == 0) spawnPosition.z += 0.5f;
        spawnPosition.y = -0.5f;

        return spawnPosition;
    }
    
    private void CreateEditorTile(int x, int y, Vector3 spawnPosition, SavedTileData? savedTileData)
    {
        GameObject created = Instantiate(_defaultTilePrefab, spawnPosition, Quaternion.identity, _tileContainer.transform);
        Undo.RegisterCreatedObjectUndo(created, "Create Tile Object");
        
        Tile tile = created.GetComponent<Tile>();
        if (!tile)
        {
            Debug.LogError($"Board Build Error: Couldn't build tile at ({x}, {y}) as prefab missing Tile component.");
            return;
        }

        _mapData.Tiles[x, y] = tile;

        if (savedTileData.HasValue)
        {
            // Extract the actual struct data safely using .Value
            SavedTileData tileData = savedTileData.Value;
            tile.RegionID = tileData.Region;
            tile.Data = TileRegistryV2.Instance.GetTileData(tileData.TileID);
        }

        tile.Init();
        
        if (_showTileDataCreation) Debug.Log($"Created tile data for tile positioned at ({x}, {y})");
    }
    
    public void ConstructSaveData()
    {
        string filePath = $"Assets/{_fileName}.asset";
        
        SavedMapData savedMapData = ScriptableObject.CreateInstance<SavedMapData>();
        
        savedMapData.MapSize = _mapData.MapSize;
        savedMapData.Tiles = new SavedTileData[_mapData.MapSize.x * _mapData.MapSize.y];
        
        for (int x = 0; x < _mapData.MapSize.x; x++)
        {
            for (int y = 0; y < _mapData.MapSize.y; y++)
            {
                int flatIndex = x + (y * _mapData.MapSize.x);
                Tile currentTile = _mapData.Tiles[x, y];
                if (currentTile != null)
                {
                    savedMapData.Tiles[flatIndex] = new SavedTileData
                    {
                        Region = currentTile.RegionID,
                        TileID = currentTile.Data != null ? currentTile.Data.ID : string.Empty
                    };
                }
            }
        }
        
        AssetDatabase.CreateAsset(savedMapData, filePath);
        
        EditorUtility.SetDirty(savedMapData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Saved saved map to {filePath}!");
    }
}
#endif

using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public class BoardBuilder : MonoBehaviour
{
    private const string DEFAULT_BOARD_NAME = "New Board Asset";
    
    [Header("References")]
    [SerializeField] private Grid _grid;
    [SerializeField] private Transform _shaderPlane;
    
    [Header("Tile Prefab")]
    [SerializeField] private GameObject _defaultTilePrefab;
    
    [Header("Build Settings")]
    [SerializeField] private MapData _mapData; // Struct type
    [SerializeField] private string _fileName = DEFAULT_BOARD_NAME;

    [Header("Debug")]
    [SerializeField] private bool _showTileDataCreation;
    
    private GameObject _prevTileContainer;
    private GameObject _tileContainer;
    private Vector3Int _bottomLeftTileOffset;
    
    private SavedMapData _loadedMapAsset;
    private string _loadedAssetPath;
     
    private void Resize()
    {
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

        if (!mapToLoad)
        {
            Debug.LogError($"BoardBuilder Load Error: No SavedMapData found at path {filePath}");
            return;
        }
        
        _loadedMapAsset = mapToLoad;
        _loadedAssetPath = filePath;
        _fileName = mapToLoad.name;

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
        _loadedMapAsset = null;
        _loadedAssetPath = string.Empty;

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
        Quaternion spawnRotation = Quaternion.identity;
        
        if (savedTileData.HasValue)
        {
            spawnRotation = savedTileData.Value.Rotation;
        }
        
        GameObject created = Instantiate(_defaultTilePrefab, spawnPosition, spawnRotation, _tileContainer.transform);
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
            SavedTileData tileData = savedTileData.Value;
            tile.Data = TileRegistry.Instance.GetTileData(tileData.TileID);
            
            tile.IsZoneCenter = tileData.IsZoneCenter;
            tile.AssignedZone = tileData.ZoneDefinition;
            tile.DecorationProfile = tileData.DecorationProfile;
            
            tile.IsSpawnTile = tileData.IsSpawnTile;
            tile.SpawnOwnerID = tileData.SpawnOwnerID;
        }

        tile.Init();
        
        if (_showTileDataCreation) Debug.Log($"Created tile data for tile positioned at ({x}, {y})");
    }
    
    public void ConstructSaveData()
    {
        string targetPath;
        SavedMapData mapToSave;

        // If a board was previously loaded, overwrite it
        if (_loadedMapAsset && !string.IsNullOrEmpty(_loadedAssetPath))
        {
            targetPath = _loadedAssetPath;
            mapToSave = _loadedMapAsset;
        }
        else
        {
            if (string.IsNullOrEmpty(_fileName))
            {
                _fileName = DEFAULT_BOARD_NAME;
            }
            
            targetPath = $"Assets/{_fileName}.asset";
            
            mapToSave = AssetDatabase.LoadAssetAtPath<SavedMapData>(targetPath);
            if (!mapToSave)
            {
                mapToSave = ScriptableObject.CreateInstance<SavedMapData>();
                AssetDatabase.CreateAsset(mapToSave, targetPath);
            }
        }

        mapToSave.Initialise(_mapData.MapSize, _mapData.Tiles);
        
        EditorUtility.SetDirty(mapToSave);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        _loadedMapAsset = mapToSave;
        _loadedAssetPath = targetPath;
        
        Debug.Log($"Saved board to {targetPath}!");
    }
}
#endif
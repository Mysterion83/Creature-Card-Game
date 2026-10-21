using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
// NOTES
// Board still needs an active runtime state
// That way easier to track units, functions are present for finding positions and Tile Data
// Now just needs tile data updating and definition at runtime through board controller

public class BoardBuilder : MonoBehaviour
{
    [Header("Board References")]
    [SerializeField] private Grid _grid;
    [SerializeField] private Transform _shaderPlane;
    
    [Header("Build Settings")]
    [SerializeField] private MapData _mapData;
    [SerializeField] private string _fileName = "New MapSaveData";

    [Header("Debug")]
    [SerializeField] private bool _showTileDataCreation = false;
    
    [Header("Test Tile")]
    [SerializeField] private GameObject _defaultTilePrefab;
    
    private GameObject _prevTileContainer;
    private GameObject _tileContainer;
    // Variable to set the bottom left grid tile to be (0, 0) instead of the centre
    private Vector3Int _bottomLeftTileOffset;
    // Used for validation to auto-build grid
    private Vector2Int _previousGridSize;

    private SavedTileData[,] _tilesToLoad;
     
    private void Resize()
    {
        if (_previousGridSize == _mapData.MapSize) return;
        
        _previousGridSize = _mapData.MapSize;
        
        _bottomLeftTileOffset = new Vector3Int(-(_mapData.MapSize.x / 2), -(_mapData.MapSize.y / 2), 0);
                
        EditorApplication.delayCall+=()=>
        {
            DestroyImmediate(_prevTileContainer);
        };

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
            Debug.LogError($"\"BoardBuilder Load Error: Couldn't load asset at path {filePath}, object type is not SavedMapData");
            return;
        }

        _tilesToLoad = new SavedTileData[mapToLoad.MapSize.x, mapToLoad.MapSize.y];
        
        CustomBuild(mapToLoad);
    }
    
    public void DestroyTiles()
    {
        EditorApplication.delayCall+=()=>
        {
            DestroyImmediate(_tileContainer);
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
        
        for (int i = 0; i < _mapData.Tiles.GetLength(0); i++)
        {
            for (int j = 0; j < _mapData.Tiles.GetLength(1); j++)
            {
                Vector3 spawnPosition = _grid.CellToWorld(new Vector3Int(i, j) + _bottomLeftTileOffset);
                
                if (_mapData.MapSize.x % 2 == 0) spawnPosition.x += 0.5f;
                if (_mapData.MapSize.y % 2 == 0) spawnPosition.z += 0.5f;
                spawnPosition.y = -0.5f;
                
                GameObject created = Instantiate(_defaultTilePrefab, 
                    spawnPosition,  
                    Quaternion.identity,
                    _tileContainer.transform);
                
                Undo.RegisterCreatedObjectUndo(created, "Create Tile Object");
                
                created.transform.SetParent(_tileContainer.transform);
                Tile tile = created.GetComponent<Tile>();

                if (!tile)
                {
                    Debug.LogError($"Board Build Error: Couldn't build tiles as prefab object did not contain a Tile component");
                    return;
                }

                _mapData.Tiles[i, j] = created.GetComponent<Tile>();;
                tile.Init();
                
                if (!_showTileDataCreation) continue; 
                Debug.Log($"Created tile data for tile positioned at ({i}, {j})");
            }
        }
    }

    private void CustomBuild(SavedMapData mapToLoad)
    {
        _mapData.MapSize = mapToLoad.MapSize;
        _mapData.Tiles = new Tile[_mapData.MapSize.x, _mapData.MapSize.y];

        _prevTileContainer = _tileContainer;
        _tileContainer = new GameObject("Tile Container");
        _tileContainer.transform.SetParent(transform);
        
        if (_prevTileContainer) Undo.DestroyObjectImmediate(_prevTileContainer);
        Undo.RegisterCreatedObjectUndo(_tileContainer, "Instantiate Tile Container");
        
        // Convert 1D array to 2D array of Tiles
        _tilesToLoad = new SavedTileData[mapToLoad.MapSize.x, mapToLoad.MapSize.y];
        for (int i = 0; i < _tilesToLoad.GetLength(0); i++)
        {
            for (int j = 0; j < _tilesToLoad.GetLength(1); j++)
            {
                _tilesToLoad[i, j] = mapToLoad.Tiles[i * mapToLoad.MapSize.x + j];
            }
        }
        
        // Loop through 2D array of tiles and build them
        for (int i = 0; i < _tilesToLoad.GetLength(0); i++)
        {
            for (int j = 0; j < _tilesToLoad.GetLength(1); j++)
            {
                Vector3 spawnPosition = _grid.CellToWorld(new Vector3Int(i, j) + _bottomLeftTileOffset);
                
                if (_mapData.MapSize.x % 2 == 0) spawnPosition.x += 0.5f;
                if (_mapData.MapSize.y % 2 == 0) spawnPosition.z += 0.5f;
                spawnPosition.y = -0.5f;
                
                GameObject created = Instantiate(_defaultTilePrefab, 
                    spawnPosition,  
                    Quaternion.identity,
                    _tileContainer.transform);
                
                Undo.RegisterCreatedObjectUndo(created, "Create Tile Object");
                
                created.transform.SetParent(_tileContainer.transform);
                Tile tile = created.GetComponent<Tile>();

                if (!tile)
                {
                    Debug.LogError($"Board Build Error: Couldn't build tiles as prefab object did not contain a Tile component");
                    return;
                }

                _mapData.Tiles[i, j] = tile;

                tile.Data = TestTileRegistry.Instance.GetTileData(_tilesToLoad[i,j].TileID);
                tile.Init();
                
                if (!_showTileDataCreation) continue; 
                Debug.Log($"Created tile data for tile positioned at ({i}, {j})");
            }
        }
        
        Resize();
    }
    
    
    
    public void ConstructSaveData()
    {
        string filePath = $"Assets/{_fileName}.asset";

        // Attempt to Load, will return null if asset isn't found
        SavedMapData savedMapData = AssetDatabase.LoadAssetAtPath<SavedMapData>(filePath);
        
        // Create asset and Initialise
        savedMapData = ScriptableObject.CreateInstance<SavedMapData>();
        AssetDatabase.CreateAsset(savedMapData, filePath);
        savedMapData.Initialise(_mapData.MapSize, _mapData.Tiles);
        
        // Mark dirty to save changes to disk
        EditorUtility.SetDirty(savedMapData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    public Tile GetTileDataFromPosition(Vector3 position)
    {
        Vector3Int cellPosition = _grid.WorldToCell(position);
        Vector3Int localCellPosition = cellPosition - _bottomLeftTileOffset;

        return _mapData.Tiles[localCellPosition.x, localCellPosition.y];
    }
}
#endif

[Serializable]
public struct MapData
{
    [SerializeField, Min(2)] public Vector2Int MapSize;
    public Tile[,] Tiles;

    public MapData(Vector2Int mapSize, Tile[,] tiles)
    {
        MapSize = mapSize;
        Tiles = tiles;
    }
}
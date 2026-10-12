#if UNITY_EDITOR
using System;
using UnityEditor;
#endif
using UnityEngine;

// NOTES
// Board still needs an active runtime state
// That way easier to track units, functions are present for finding positions and Tile Data
// Now just needs tile data updating and definition at runtime through board controller

public class Board : MonoBehaviour
{
    [Header("Board References")]
    [SerializeField] private Grid _grid;
    [SerializeField] private Transform _shaderPlane;
    [SerializeField] private Material _gridShaderMaterial;
    
    [Header("Grid Settings")]
    [SerializeField, Min(2)] private Vector2Int _gridSize = new Vector2Int(20, 20);
    [SerializeField, Range(0.01f, 0.1f)] private float _cellThickness = 0.1f;
    [SerializeField] private float _gridYOffset = 0.015f;
    [SerializeField] private Color _gridColour = Color.white;

    [Header("Debug")]
    [SerializeField] private bool _showTileDataCreation = false;
    
    [Header("Test Tile")]
    [SerializeField] private GameObject _tilePrefab;

    [SerializeField] private MapData _mapData;

    // 2D array to store Tile Information
    private Tile[,] _tiles;
    private GameObject _prevTileContainer;
    private GameObject _tileContainer;
    // Variable to set the bottom left grid tile to be (0, 0) instead of the centre
    private Vector3Int _bottomLeftTileOffset;
    // Used for validation to auto-build grid
    private Vector2Int _previousGridSize;
    
    private void OnValidate()
    {
        if (_previousGridSize == _gridSize) return;
        
        _previousGridSize = _gridSize;
        
        _bottomLeftTileOffset = new Vector3Int(-(_gridSize.x / 2), -(_gridSize.y / 2), 0);
        
        EditorApplication.delayCall+=()=>
        {
            DestroyImmediate(_prevTileContainer);
        };
        
        // Move strings to const class
        _shaderPlane.localScale = new Vector3(_gridSize.x * 0.1f, 1, _gridSize.y * 0.1f);
        _shaderPlane.position = new Vector3(0, _gridYOffset, 0);
        //_gridShader.SetVector("_Size", new Vector2(_gridSize.x, _gridSize.y)); // Vector2 to convert into a Vector4 from Vector2Int
        _gridShaderMaterial.SetFloat("_Thickness", _cellThickness);
        _gridShaderMaterial.SetColor("_Colour", _gridColour);
    }
    
    public void Build()
    {
        _tiles = new Tile[_gridSize.x, _gridSize.y];

        _prevTileContainer = _tileContainer;
        _tileContainer = new GameObject("Tile Container");
        _tileContainer.transform.SetParent(transform);
        
        if (_prevTileContainer) Undo.DestroyObjectImmediate(_prevTileContainer);
        Undo.RegisterCreatedObjectUndo(_tileContainer, "Instantiate Tile Container");
        
        for (int i = 0; i < _tiles.GetLength(0); i++)
        {
            for (int j = 0; j < _tiles.GetLength(1); j++)
            {
                Vector3 spawnPosition = _grid.CellToWorld(new Vector3Int(i, j) + _bottomLeftTileOffset);
                
                if (_gridSize.x % 2 == 0) spawnPosition.x += 0.5f;
                if (_gridSize.y % 2 == 0) spawnPosition.z += 0.5f;
                spawnPosition.y = -0.5f;
                
                GameObject created = Instantiate(_tilePrefab, 
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

                _tiles[i, j] = created.GetComponent<Tile>();;
                tile.Init();
                
                if (!_showTileDataCreation) continue; 
                Debug.Log($"Created tile data for tile positioned at ({i}, {j})");
            }
        }
    }
    
    public Tile GetTileDataFromPosition(Vector3 position)
    {
        Vector3Int cellPosition = _grid.WorldToCell(position);
        Vector3Int localCellPosition = cellPosition - _bottomLeftTileOffset;

        return _tiles[localCellPosition.x, localCellPosition.y];
    }

    // Wrapper method to avoid grid component referencing
    public Vector3Int GetWorldToCell(Vector3 position)
    {
        return _grid.WorldToCell(position);
    }
}

[Serializable]
public struct MapData
{
    public Vector2Int MapSize;
    public Tile[,] Tiles;
}
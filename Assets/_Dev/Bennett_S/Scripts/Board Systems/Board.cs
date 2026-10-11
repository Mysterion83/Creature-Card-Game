using Unity.VisualScripting;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Board References")]
    [SerializeField] private Grid _grid;
    [SerializeField] private Transform _scalingParent;
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
    
    // 2D array to store Tile Information
    private Tile[,] _tiles;
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
        
        InitialiseGrid();
        
        // Move strings to const class
        _scalingParent.localScale = new Vector3(_gridSize.x * 0.1f, 1, _gridSize.y * 0.1f);
        _shaderPlane.position = new Vector3(0, _gridYOffset, 0);
        //_gridShader.SetVector("_Size", new Vector2(_gridSize.x, _gridSize.y)); // Vector2 to convert into a Vector4 from Vector2Int
        _gridShaderMaterial.SetFloat("_Thickness", _cellThickness);
        _gridShaderMaterial.SetColor("_Colour", _gridColour);
    }

    // Move this to a custom button
    private void InitialiseGrid()
    {
        _tiles = new Tile[_gridSize.x, _gridSize.y];
        
        Destroy(_tileContainer); // Does not work
        _tileContainer = new GameObject("TileContainer")
        {
            transform =
            {
                position = new Vector3(0, 0, 1)
            }
        };
        _tileContainer.transform.SetParent(transform);
        
        for (int i = 0; i < _tiles.GetLength(0); i++)
        {
            for (int j = 0; j < _tiles.GetLength(1); j++)
            {
                Vector3 spawnPosition = _grid.CellToWorld(new Vector3Int(i, j) + _bottomLeftTileOffset);
                spawnPosition.y = -0.5f;
                
                GameObject created = Instantiate(_tilePrefab, 
                    spawnPosition,  
                    Quaternion.identity,
                    _tileContainer.transform);

                _tiles[i, j] = new Tile();
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
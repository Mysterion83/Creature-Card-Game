using UnityEngine;

[RequireComponent(typeof(TileSelectionController))]
public class BoardController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputHandler _input;

    [Header("Grid References")]
    [SerializeField] private Grid _grid;
    [SerializeField] private Transform _testCreature;
    [SerializeField] private Transform _selectionBox;
    [SerializeField] private Transform _scalingParent;
    [SerializeField, Space] private Transform _shaderPlane;
    [SerializeField] private Material _gridShaderMaterial;

    [Header("Grid Settings")]
    [SerializeField, Min(2)] private Vector2Int _gridSize = new Vector2Int(20, 20);
    [SerializeField, Range(0.01f, 0.1f)] private float _cellThickness = 0.1f;
    [SerializeField] private float _gridYOffset = 0.015f;
    [SerializeField] private Color _gridColour = Color.white;

    [Header("Debug")]
    [SerializeField] private bool _showTileDataCreation = false;
    [SerializeField] private bool _enableUnitCoordinateDebug = false;

    private TileSelectionController _selectionController;

    // 2D array to store Tile Information
    private Tile[,] _tiles;

    // Variable to set the bottom left grid tile to be (0, 0) instead of the center
    private Vector3Int _bottomLeftTileOffset;

    public Vector3 SelectedPosition { get; private set;  }

    private void OnEnable()
    {
        _input.OnPlacement += PlaceUnit;
    }

    private void OnDisable()
    {
        _input.OnPlacement -= PlaceUnit;
    }

    private void OnValidate()
    {
        // Move strings to const class
        _scalingParent.localScale = new Vector3(_gridSize.x * 0.1f, 1, _gridSize.y * 0.1f);
        _shaderPlane.position = new Vector3(0, _gridYOffset, 0);
        //_gridShader.SetVector("_Size", new Vector2(_gridSize.x, _gridSize.y)); // Vector2 to convert into a Vector4 from Vector2Int
        _gridShaderMaterial.SetFloat("_Thickness", _cellThickness);
        _gridShaderMaterial.SetColor("_Colour", _gridColour);
    }

    private void Awake()
    {
        _selectionController = GetComponent<TileSelectionController>();

        InitialiseGrid();
    }

    private void Update()
    {
        SelectedPosition = _selectionController.GetSelectedTilePosition();
        Vector3Int cellPosition = _grid.WorldToCell(SelectedPosition);
        _selectionBox.transform.position = _grid.GetCellCenterWorld(cellPosition);
        _testCreature.transform.position = _grid.CellToWorld(cellPosition);
    }

    private void InitialiseGrid()
    {
        _bottomLeftTileOffset = new Vector3Int(-(_gridSize.x / 2), -(_gridSize.y / 2), 0);

        _tiles = new Tile[_gridSize.x, _gridSize.y];

        for (int i = 0; i < _tiles.GetLength(0); i++)
        {
            for (int j = 0; j < _tiles.GetLength(1); j++)
            {
                _tiles[i, j] = new Tile();
                if (!_showTileDataCreation) continue; 
                Debug.Log($"Created tile data for tile positioned at ({i}, {j})");
            }
        }
    }

    public Tile GetTileDataFromPosition()
    {
        Vector3Int cellPosition = _grid.WorldToCell(SelectedPosition);
        Vector3Int localCellPosition = cellPosition - _bottomLeftTileOffset;

        return _tiles[localCellPosition.x, localCellPosition.y];
    }

    private void PlaceUnit()
    {
        Tile tileInfo = GetTileDataFromPosition();
        if (tileInfo.Occupant)
        {
            if (_enableUnitCoordinateDebug)
            {
                Debug.Log($"Placement position: {_grid.WorldToCell(SelectedPosition) - _bottomLeftTileOffset} | Array Space | {_grid.WorldToCell(SelectedPosition)} | Grid Component Space | is occupied, returning.");
            }
            return;
        }

        tileInfo.Occupant = Instantiate(_testCreature, _selectionBox.transform.position, Quaternion.identity).gameObject;
    }
}
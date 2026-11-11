#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[System.Serializable]
[ExecuteAlways]
public class Tile : MonoBehaviour
{
    [Header("Tile Definition")] 
    public TileData Data;
    
    [Header("Tile Info")] 
    public Creature Occupant;
    public Vector2Int TileIndex;
    
    [Header("Zone Info")]
    public int ZoneID = -1;
    public bool IsZoneCenter;
    public ZoneDefinition AssignedZone;
    
    [Header("Spawn Info")]
    public bool IsSpawnTile;
    public int SpawnOwnerID = -1;
    
    private TileData _prevData;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    private GameObject _renderObject;

    private bool _queuedForDecorationRefresh;

    private void OnValidate()
    {
        if (Application.isPlaying) return;
        
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        
        if (Data == _prevData) return;
        
        _queuedForDecorationRefresh = true;

#if UNITY_EDITOR
        EditorApplication.update -= HandleEditorUpdate;
        EditorApplication.update += HandleEditorUpdate;
#endif
    }

#if UNITY_EDITOR
    private void HandleEditorUpdate()
    {
        // Unsubscribe to prevent infinite execution
        EditorApplication.update -= HandleEditorUpdate;

        if (!_queuedForDecorationRefresh || !this) return;
        _queuedForDecorationRefresh = false;
        
        Init();
    }
#endif

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Init()
    {
        ClearDecorations();
        
        if (!Data) return;
        _prevData = Data;

        if (!Data.RenderObjectPrefab)
        {
            Debug.LogWarning(
                $"Tiles using {Data} have no render object prefab, please assign one in {Data}'s Inspector");
            return;
        }
        
#if UNITY_EDITOR
        GameObject targetSource = Data.RenderObjectPrefab;
        
        if (!Application.isPlaying)
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(Data.RenderObjectPrefab);
            if (source)
            {
                targetSource = source;
            }
        }
        
        // Mesh and Material assignment
        _meshFilter.sharedMesh = targetSource.TryGetComponent<MeshFilter>(out var prefabMeshFilter) 
            ? prefabMeshFilter.sharedMesh : null;
#endif
        
        if (Application.isPlaying)
        {
            if (_renderObject)
            {
                Destroy(_renderObject);
                _renderObject = null;
            }
            
            if (Data.RenderObjectPrefab)
            {
                _renderObject = Instantiate(Data.RenderObjectPrefab, transform);
            }
            
            // Set mesh and material to null so the render object is visible
            if (_meshRenderer)
            {
                _meshRenderer.sharedMaterial = null;
            }
            if (_meshFilter)
            {
                _meshFilter.sharedMesh = null;
            }
        }
        
        // Tile Decoration Spawning
        if (Data.Decorations == null) return;
        
        foreach (var decoration in Data.Decorations)
        {
            decoration?.Spawn(transform);
        }
    }
    
    public void ClearDecorations()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
#if UNITY_EDITOR
                if (child)
                {
                    Undo.DestroyObjectImmediate(child);
                }
#endif
            }
        }
    }
}

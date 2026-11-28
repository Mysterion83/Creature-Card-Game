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
    public TileDecorationProfile DecorationProfile;
    
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
        if (!_meshFilter) _meshFilter = GetComponent<MeshFilter>();
        if (!_meshRenderer) _meshRenderer = GetComponent<MeshRenderer>();

        ClearDecorations();
        _prevData = Data;

        if (Data)
        {
            if (Data.IsEmpty)
            {
                ConfigureEmptyTile();
            }
            else
            {
                ConfigureTile();
            }
        }

        SpawnDecorations();
    }

    private void ConfigureEmptyTile()
    {
        GameObject targetSource = Data.RenderObjectPrefab;

#if UNITY_EDITOR
        if (!Application.isPlaying && targetSource)
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(targetSource);
            if (source) targetSource = source;

            ApplyEditorMeshAndMaterial(targetSource, Data.EditorMaterial, true);
            return;
        }
#endif

        // Disable mesh at runtime
        if (!Application.isPlaying) return;
        
        if (_meshFilter) _meshFilter.sharedMesh = null;
        if (_meshRenderer) _meshRenderer.enabled = false;
    }

    private void ConfigureTile()
    {
        if (!Data.RenderObjectPrefab)
        {
            Debug.LogWarning($"Tiles using {Data} have no render object prefab, please assign one in {Data}'s Inspector");
            return;
        }

        GameObject targetSource = Data.RenderObjectPrefab;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(targetSource);
            if (source) targetSource = source;
            
            ApplyEditorMeshAndMaterial(targetSource, Data.EditorMaterial, true);
            return;
        }
#endif

        if (Application.isPlaying)
        {
            ApplyRuntimeMeshAndMaterial(targetSource);
        }
    }

    private void ApplyEditorMeshAndMaterial(GameObject source, Material material, bool enableRenderer)
    {
        if (_meshFilter)
        {
            _meshFilter.sharedMesh = source.TryGetComponent<MeshFilter>(out var mf) ? mf.sharedMesh : null;
        }

        if (!_meshRenderer) return;
        
        _meshRenderer.sharedMaterial = material;
        _meshRenderer.enabled = enableRenderer;
    }

    private void ApplyRuntimeMeshAndMaterial(GameObject source)
    {
        if (_meshFilter)
        {
            _meshFilter.sharedMesh = source.TryGetComponent<MeshFilter>(out var mf) ? mf.sharedMesh : null;
        }

        if (!_meshRenderer) return;
        
        _meshRenderer.sharedMaterial = source.TryGetComponent<MeshRenderer>(out var mr) ? mr.sharedMaterial : null;
        _meshRenderer.enabled = true;
    }

    private void SpawnDecorations()
    {
        if (!DecorationProfile || DecorationProfile.Decorations == null) return;

        foreach (var decoration in DecorationProfile.Decorations)
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

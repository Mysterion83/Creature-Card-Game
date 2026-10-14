using UnityEngine;

[System.Serializable]
public class Tile : MonoBehaviour
{
    [Header("Tile Definition")]
    public TileData Data;
    
    [Header("Active Info")]
    public int RegionID = -1;
    public Creature Occupant;

    private TileData _prevData;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    
    private void OnValidate()
    {
        if (Data == _prevData) return;
        //OnDataUpdate();
    }

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Init()
    {
        OnDataUpdate();
    }
    
    private void OnDataUpdate()
    {
        if (!Data) return;
        _prevData = Data;
        
        if (!Data.RenderObjectPrefab)
        {
            Debug.LogWarning($"Tiles using {Data} have no render object prefab, please assign one in {Data}'s Inspector");
            return;
        }

        if (Data.RenderObjectPrefab.TryGetComponent<MeshFilter>(out var meshFilter))
        {
            _meshFilter.sharedMesh = meshFilter? meshFilter.sharedMesh : null;
        }
        if (Data.RenderObjectPrefab.TryGetComponent<MeshRenderer>(out var meshRenderer))
        {
            _meshRenderer.sharedMaterial = meshFilter? meshRenderer.sharedMaterial : null;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class TileRegistry : MonoBehaviour
{
    public static TileRegistry Instance;

    [Header("Default Tiles")]
    [SerializeField] private TileData[] _defaultTiles;

    [Header("Debug")]
    [SerializeField] private bool _enableDebugLogs;
    [SerializeField] private Dictionary<GUID, TileData> _registry;

#if UNITY_EDITOR
    public void OnValidate()
    {
        Instance = this;
        Build();
    }
#endif
    
    public void Awake()
    {
        Instance = this;
        Build();
    }
    
    /// <summary>
    /// Retrieve <c>TileData</c> from a Tile's GUID
    /// </summary>
    /// <param name="id">The required tile's GUID</param>
    /// <returns>The <c>TileData</c> if found, otherwise null</returns>
    public TileData GetTileData(GUID id)
    {
        if (_registry != null && _registry.TryGetValue(id, out var tileData))
        {
            return tileData;
        }

        Debug.LogError($"TileRegistry: TileData with GUID {id} not found!");
        return null;
    }

    /// <summary>
    /// Retrieve <c>TileData</c> from a Tile's string ID
    /// </summary>
    /// <param name="stringID">The required tile's string ID</param>
    /// <returns>The <c>TileData</c> if found, otherwise null</returns>
    public TileData GetTileData(string stringID)
    {
        if (_registry != null)
        {
            foreach (var tile in _registry.Values)
            {
                if (tile && tile.StringID == stringID)
                {
                    return tile;
                }
            }
        }

        Debug.LogError($"TileRegistry: TileData with string ID '{stringID}' not found!");
        return null;
    }
    
    public void Build()
    {
        _registry ??= new Dictionary<GUID, TileData>();

        if (_defaultTiles == null)
        {
            Debug.LogError("[TileRegistry] Could not build registry as the _defaultTiles list is empty!");
            return;
        }
        
        foreach (TileData tile in _defaultTiles)
        {
            if (_registry.ContainsKey(tile.ID)) continue;
            
            if (!_registry.TryAdd(tile.ID, tile))
            {
                Debug.LogWarning($"[TileRegistry] Failed to add Tile {tile.name} to the registry!");
            }
        }
        
        if (_enableDebugLogs) Debug.Log($"[TileRegistry] Successfully built {_registry.Count} tiles.");
    }

    public void Clear()
    {
        if (_registry == null || _registry.Count <= 0)
        {
            Debug.LogWarning("[TileRegistry] Could not clear registry as there was nothing to clear!");
            return;
        }
        
        _registry.Clear();
        if (_enableDebugLogs) Debug.Log("[TileRegistry] Successfully removed all tiles from the registry.");
    }
}
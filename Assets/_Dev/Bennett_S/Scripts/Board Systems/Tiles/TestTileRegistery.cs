using System.Collections.Generic;
using UnityEngine;

public class TileRegistry : MonoBehaviour
{
    public static TileRegistry Instance;

    [SerializeField] private TileData[] _defaultTiles;

    [SerializeField] private Dictionary<string, TileData> _registry;

    public TileData GetTileData(string id)
    {
        return _registry[id];
    }

#if UNITY_EDITOR
    public void OnValidate()
    {
        Instance = this;
        BuildRegistry();
    }

#endif
    public void OnEnable()
    {
        Instance = this;
        BuildRegistry();
    }

    public void BuildRegistry()
    {
        _registry = new Dictionary<string, TileData>();
        foreach (TileData tile in _defaultTiles)
        {
            if (!_registry.TryAdd(tile.ID, tile))
            {
                Debug.LogWarning($"Failed to add Tile to registry - {tile.name}");
            }
        }
    }
}

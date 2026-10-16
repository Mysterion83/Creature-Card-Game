using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileRegistry", menuName = "Project/Registries/Tile Registry")]
public class TestTileRegistry : MonoBehaviour
{
    public static TestTileRegistry Instance;

    [SerializeField] private TileData[] _defaultTiles;

    [SerializeField] private Dictionary<string, TileData> _registry;

    public TileData GetTileData(string ID)
    {
        return _registry[ID];
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

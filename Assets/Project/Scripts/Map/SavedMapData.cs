using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Project/Map Data")]
public class SavedMapData : ScriptableObject
{
    const ushort CURRENT_MAP_VERSION = 1;

    public ushort MapVersion = CURRENT_MAP_VERSION;

    public string MapName;
    
    /// <summary>
    /// The Size of the map
    /// </summary>
    public Vector2Int MapSize;

    /// <summary>
    /// Array of saved tiles
    /// </summary>
    public SavedTileData[] Tiles;
    
    /// <summary>
    /// Initialises the map data with the specified size and tiles.
    /// </summary>
    /// <param name="mapSize"></param>
    /// <param name="tiles"></param>
    public void Initialise(Vector2Int mapSize, Tile[,] tiles)
    {
        MapVersion = CURRENT_MAP_VERSION;
        MapSize = mapSize;
        Tiles = new SavedTileData[mapSize.x * mapSize.y];

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                int flatIndex = x + y * mapSize.x;
                Tile item = tiles[x, y];

                if (item && item.Data)
                {
                    Tiles[flatIndex] = new SavedTileData(
                        item.Data.ID, item.ZoneID, item.IsZoneCenter, 
                        item.AssignedZone, item.IsSpawnTile, item.SpawnOwnerID,
                        item.transform.rotation);
                }
            }
        }
    }
}
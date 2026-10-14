using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Project/Map Data")]
public class SavedMapData : ScriptableObject
{
    const ushort CURRENT_MAP_VERSION = 1;

    public ushort MapVersion = CURRENT_MAP_VERSION;

    public string MapName;

    public Vector2Int MapSize;

    public SavedTileData[] Tiles;


    [Obsolete("Unfinished Function")]
    public SavedMapData(Vector2Int mapSize, Tile[,] tiles)
    {
        MapVersion = CURRENT_MAP_VERSION;
        MapSize = mapSize;
        List<SavedTileData> tilesToSave = new List<SavedTileData>();

        foreach (Tile item in tiles)
        {
            SavedTileData tileData = new SavedTileData(item.RegionID, item.Data.ID);
            tilesToSave.Add(tileData);
        }
        Tiles = tilesToSave.ToArray();
    }

    
}
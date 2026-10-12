using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Scriptable Objects/MapData")]
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

    [Obsolete("Unfinished Function")]
    public static MapData LoadMapData(SavedMapData data)
    {
        MapData mapData = new MapData();

        mapData.MapSize = data.MapSize;
        mapData.Tiles = new Tile[data.MapSize.x, data.MapSize.y];
        for (int currentTile = 0; currentTile < data.Tiles.Length; currentTile++)
        {
            int CurrentRow = currentTile / mapData.MapSize.x;
            mapData.Tiles[currentTile % mapData.MapSize.x, CurrentRow] = new Tile
            {
                RegionID = data.Tiles[currentTile].Region,
                //PLACEHOLDER - IMPLEMENT ASAP - need to implement a registry of tile data
                Data = TileRegistry.Instance.GetTileData(data.Tiles[currentTile].TileID)
            };
        }

        return mapData;
    }
}
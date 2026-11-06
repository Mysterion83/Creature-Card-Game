using System;

[Serializable]
public struct SavedTileData
{
    public string TileID;
    
    public int ZoneID;
    public bool IsZoneCenter;
    public ZoneDefinition ZoneDefinition;

    public bool IsSpawnTile;
    public int SpawnOwnerID;
    
    public SavedTileData(string tileID, int zoneID, bool isZoneCenter, ZoneDefinition zoneDefinition, bool isSpawnTile, int spawnOwnerID )
    {
        TileID = tileID;
        ZoneID = zoneID;
        IsZoneCenter = isZoneCenter;
        ZoneDefinition = zoneDefinition;
        IsSpawnTile = isSpawnTile;
        SpawnOwnerID = spawnOwnerID;
    }
}
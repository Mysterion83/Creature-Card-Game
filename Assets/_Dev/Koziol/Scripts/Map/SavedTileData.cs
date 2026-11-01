using System;

[Serializable]
public struct SavedTileData
{
    public string TileID;
    public int ZoneID;
    public bool IsZoneCenter;
    public ZoneDefinition ZoneDefinition;
    
    public SavedTileData(string tileID, int zoneID, bool isZoneCenter , ZoneDefinition zoneDefinition )
    {
        TileID = tileID;
        ZoneID = zoneID;
        IsZoneCenter = isZoneCenter;
        ZoneDefinition = zoneDefinition;
    }
}
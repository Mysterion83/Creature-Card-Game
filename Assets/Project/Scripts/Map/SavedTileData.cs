using System;
using UnityEngine;

[Serializable]
public struct SavedTileData
{
    /// <summary>
    /// Tile GUID being saved
    /// </summary>
    public GUID TileID;
    /// <summary>
    /// Zome of the Tile
    /// </summary>
    public int ZoneID;
    public bool IsZoneCenter;
    public ZoneDefinition ZoneDefinition;

    /// <summary>
    /// Can Creatures Spawn in the Tile
    /// </summary>
    public bool IsSpawnTile;
    /// <summary>
    /// Who owns the spawner
    /// </summary>
    public int SpawnOwnerID;
    
    public Quaternion Rotation;
    
    public TileDecorationProfile DecorationProfile;
    
    public SavedTileData(GUID tileID, int zoneID, bool isZoneCenter, ZoneDefinition zoneDefinition, bool isSpawnTile, int spawnOwnerID, Quaternion spawnRotation, TileDecorationProfile decorationProfile )
    {
        TileID = tileID;
        ZoneID = zoneID;
        IsZoneCenter = isZoneCenter;
        ZoneDefinition = zoneDefinition;
        IsSpawnTile = isSpawnTile;
        SpawnOwnerID = spawnOwnerID;
        Rotation = spawnRotation;
        DecorationProfile = decorationProfile;
    }
}
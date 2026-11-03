using System.Collections.Generic;

/// <summary>
/// Runtime instance of a <c>ZoneDefinition</c>
/// </summary>
[System.Serializable]
public class ZoneInstance
{
    public int ZoneID;
    public ZoneDefinition Definition;
    public Tile CenterTile;
    public List<Tile> RelativeTiles = new();
    public Player Occupier;

    public ZoneInstance(int zoneID, ZoneDefinition definition, Tile centerTile)
    {
        ZoneID = zoneID;
        Definition = definition;
        CenterTile = centerTile;
    }
    
    public void SetOccupier(Player player) => Occupier = player;
}
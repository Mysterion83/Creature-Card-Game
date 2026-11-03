using System.Collections.Generic;

/// <summary>
/// Runtime instance of a <c>ZoneDefinition</c>
/// </summary>
[System.Serializable]
public class ZoneInstance
{
    public int InstanceID;
    public ZoneDefinition Definition;
    public Tile CenterTile;
    public List<Tile> RelativeTiles = new();
    public Player Occupier;

    public ZoneInstance(int instanceID, ZoneDefinition definition, Tile centerTile)
    {
        InstanceID = instanceID;
        Definition = definition;
        CenterTile = centerTile;
    }
    
    public void SetOccupier(Player player) => Occupier = player;
}
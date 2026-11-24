using System.Collections.Generic;
using UnityEngine;

public class ZoneMap
{
    private readonly BattleAdapter _adapter;
    private readonly List<ZoneInstance> _zones = new();
    private readonly Dictionary<ZoneInstance, bool> _threatened = new();

    public ZoneMap(BattleAdapter adapter, ThreatMap threat)
    {
        _adapter = adapter;

        foreach (ZoneInstance zone in adapter.Zones)
        {
            if (zone == null || zone.Definition == null) continue;
            _zones.Add(zone);

            bool threatened = false;
            foreach (Tile tile in zone.RelativeTiles)
                if (threat.EnemyCanReach(tile)) { threatened = true; break; }
            _threatened[zone] = threatened;
        }
    }

    public float ControlChange(Tile from, Tile to, Creature killed)
    {
        float change = 0f;
        foreach (ZoneInstance zone in _zones)
            change += (ValueAfter(zone, from, to, killed) - ValueOf(zone.Occupier)) * Scale(zone);
        return change;
    }

    public int DistanceToTarget(Tile tile)
    {
        int best = -1;
        foreach (ZoneInstance zone in _zones)
        {
            if (_adapter.IsMine(zone.Occupier)) continue;
            foreach (Tile zoneTile in zone.RelativeTiles)
            {
                if (zoneTile == null) continue;
                int distance = ActionScorer.Distance(tile.TileIndex, zoneTile.TileIndex);
                if (best < 0 || distance < best) best = distance;
            }
        }
        return best;
    }

    public float GuardValue(Tile tile)
    {
        float value = 0f;
        foreach (ZoneInstance zone in _zones)
            if (_threatened[zone] && _adapter.IsMine(zone.Occupier) && zone.RelativeTiles.Contains(tile))
                value += Scale(zone);
        return value;
    }

    private int ValueAfter(ZoneInstance zone, Tile from, Tile to, Creature killed)
    {
        int mine = 0;
        var others = new Dictionary<Player, int>();

        foreach (Tile tile in zone.RelativeTiles)
        {
            if (tile == null) continue;
            if (tile == to) { mine++; continue; }

            Creature creature = tile == from ? null : tile.Occupant;
            if (creature == null || creature == killed) continue;

            if (_adapter.IsMine(creature.Owner)) mine++;
            else others[creature.Owner] = others.TryGetValue(creature.Owner, out int count) ? count + 1 : 1;
        }

        if (mine == 0 && others.Count == 0) return ValueOf(zone.Occupier);

        int bestOther = 0;
        int leaders = 0;
        foreach (int count in others.Values)
        {
            if (count > bestOther) { bestOther = count; leaders = 1; }
            else if (count == bestOther) leaders++;
        }

        if (mine > bestOther) return 1;
        if (mine == bestOther || leaders > 1) return 0;
        return -1;
    }

    private int ValueOf(Player occupier)
    {
        if (occupier == null) return 0;
        return _adapter.IsMine(occupier) ? 1 : -1;
    }

    private static float Scale(ZoneInstance zone) => zone.Definition.BonusEnergy / 5f;
}

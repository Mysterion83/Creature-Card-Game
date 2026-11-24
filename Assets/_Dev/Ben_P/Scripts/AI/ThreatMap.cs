using System.Collections.Generic;
using UnityEngine;

/*
 * ThreatMap works out which tiles the enemy could hit on their next turn. For every
 * enemy it takes the tile it stands on plus every tile it could move to, then marks
 * every tile its attack pattern reaches from there. DamageAt adds up the biggest hit
 * each enemy could land on a creature standing on a tile.
 */

public class ThreatMap
{
    private readonly BattleAdapter _adapter;
    private readonly Dictionary<Vector2Int, List<(Creature Attacker, Tile From)>> _hits = new();

    public ThreatMap(BattleAdapter adapter, List<Creature> enemies)
    {
        _adapter = adapter;

        foreach (Creature enemy in enemies)
        {
            if (enemy == null || enemy.OccupyingTile == null) continue;

            var positions = new List<Tile> { enemy.OccupyingTile };
            positions.AddRange(adapter.MoveTilesFrom(enemy, enemy.OccupyingTile.TileIndex));

            foreach (Tile from in positions)
            {
                foreach (Vector2Int offset in enemy.ShowValidAttacks())
                {
                    Vector2Int target = from.TileIndex + offset;
                    if (!_hits.TryGetValue(target, out var list)) _hits[target] = list = new();
                    list.Add((enemy, from));
                }
            }
        }
    }

    public bool EnemyCanHit(Tile tile) => tile != null && _hits.ContainsKey(tile.TileIndex);

    public float DamageAt(Tile tile, CreatureData defender, Creature ignore = null)
    {
        if (tile == null || !_hits.TryGetValue(tile.TileIndex, out var list)) return 0f;

        var biggestHit = new Dictionary<Creature, float>();
        foreach (var (attacker, from) in list)
        {
            if (attacker == null || attacker == ignore) continue;
            float damage = _adapter.DamageAgainst(attacker, from, defender);
            if (!biggestHit.TryGetValue(attacker, out float current) || damage > current) biggestHit[attacker] = damage;
        }

        float total = 0f;
        foreach (float damage in biggestHit.Values) total += damage;
        return total;
    }
}

using System.Collections.Generic;
using UnityEngine;

/*
 * ActionScorer gives each action one number using the weights in AIProfile. Each
 * term is roughly 0 to 1 and is multiplied by its weight, then they are added up.
 * Distance is counted in grid steps (x difference plus y difference).
 */

public static class ActionScorer
{
    public static float Score(AIAction action, BattleAdapter adapter, AIProfile profile, List<Creature> enemies)
    {
        switch (action.Type)
        {
            case AIActionType.Summon:
                return profile.Summon + profile.Advance * Closeness(action.Tile.TileIndex, adapter, enemies);
            case AIActionType.Move:
                return MoveScore(action.Actor, action.Tile, profile, enemies);
            case AIActionType.Attack:
                return AttackScore(action.Actor, action.Actor.OccupyingTile, action.Tile, adapter, profile, enemies);
            default:
                return MoveScore(action.Actor, action.Tile, profile, enemies)
                    + AttackScore(action.Actor, action.Tile, action.AttackTarget, adapter, profile, enemies);
        }
    }

    private static float AttackScore(Creature attacker, Tile fromTile, Tile targetTile, BattleAdapter adapter, AIProfile profile, List<Creature> enemies)
    {
        Creature target = targetTile.Occupant;
        if (target == null) return 0f;

        float damage = adapter.PredictDamageFrom(attacker, fromTile, target);
        if (damage <= 0f) return 0f;

        float score = profile.Damage * Mathf.Min(1f, damage / target.CurrentHealth);

        if (damage >= target.CurrentHealth)
        {
            score += profile.Kill;
            if (enemies.Count == 1 && !adapter.EnemyHasCardsLeft()) score += profile.LastEnemy;
        }

        float element = adapter.ElementModifier(attacker.Data.Element, target.Data.Element);
        score += profile.ElementAdvantage * (element - 1f) * 2f;

        score += profile.TileBonus * (adapter.TileModifier(attacker, fromTile) - 1f) / 0.3f;
        return score;
    }

    private static float MoveScore(Creature creature, Tile to, AIProfile profile, List<Creature> enemies)
    {
        if (enemies.Count == 0) return 0f;
        int before = DistanceToNearest(creature.OccupyingTile.TileIndex, enemies);
        int after = DistanceToNearest(to.TileIndex, enemies);
        return profile.Advance * (before - after);
    }

    private static float Closeness(Vector2Int tile, BattleAdapter adapter, List<Creature> enemies)
    {
        Vector2Int size = adapter.BoardSize;
        float maxDistance = Mathf.Max(1, size.x + size.y);
        int distance = enemies.Count > 0 ? DistanceToNearest(tile, enemies) : Distance(tile, adapter.BoardCentre);
        return 1f - Mathf.Min(1f, distance / maxDistance);
    }

    public static int Distance(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    public static int DistanceToNearest(Vector2Int from, List<Creature> enemies)
    {
        int best = int.MaxValue;
        foreach (Creature enemy in enemies)
        {
            if (enemy == null || enemy.OccupyingTile == null) continue;
            best = Mathf.Min(best, Distance(from, enemy.OccupyingTile.TileIndex));
        }
        return best;
    }
}

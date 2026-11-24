using System.Collections.Generic;
using UnityEngine;

/*
 * ActionScorer gives each action one number using the weights in AIProfile. Each
 * term is roughly 0 to 1 and is multiplied by its weight, then they are added up.
 * Distance is counted in grid steps (x difference plus y difference).
 */

public static class ActionScorer
{
    public static float Score(AIAction action, BattleAdapter adapter, AIProfile profile, List<Creature> enemies, ThreatMap threat)
    {
        var parts = new List<(string Name, float Value)>();

        switch (action.Type)
        {
            case AIActionType.Summon:
                parts.Add(("Summon", profile.Summon));
                parts.Add(("Advance", profile.Advance * Closeness(action.Tile.TileIndex, adapter, enemies)));
                AddDefence(parts, null, action.Card, null, action.Tile, null, adapter, profile, threat);
                break;

            case AIActionType.Move:
                parts.Add(("Advance", MoveScore(action.Actor, action.Tile, profile, enemies)));
                AddDefence(parts, action.Actor, action.Actor.Data, action.Actor.OccupyingTile, action.Tile, null, adapter, profile, threat);
                break;

            case AIActionType.Attack:
                AddAttack(parts, action.Actor, action.Actor.OccupyingTile, action.Tile, adapter, profile, enemies);
                break;

            default:
                parts.Add(("Advance", MoveScore(action.Actor, action.Tile, profile, enemies)));
                Creature killed = AddAttack(parts, action.Actor, action.Tile, action.AttackTarget, adapter, profile, enemies);
                AddDefence(parts, action.Actor, action.Actor.Data, action.Actor.OccupyingTile, action.Tile, killed, adapter, profile, threat);
                break;
        }

        float total = 0f;
        string breakdown = "";
        foreach (var (name, value) in parts)
        {
            total += value;
            if (Mathf.Abs(value) < 0.05f) continue;
            breakdown += breakdown == "" ? $"{name} {value:0.0}" : $" {(value < 0 ? "-" : "+")} {name} {Mathf.Abs(value):0.0}";
        }
        action.Breakdown = breakdown;
        return total;
    }

    private static Creature AddAttack(List<(string, float)> parts, Creature attacker, Tile fromTile, Tile targetTile, BattleAdapter adapter, AIProfile profile, List<Creature> enemies)
    {
        Creature target = targetTile.Occupant;
        if (target == null) return null;

        float damage = adapter.PredictDamageFrom(attacker, fromTile, target);
        if (damage <= 0f) return null;

        parts.Add(("Damage", profile.Damage * Mathf.Min(1f, damage / target.CurrentHealth)));

        bool kills = damage >= target.CurrentHealth;
        if (kills)
        {
            parts.Add(("Kill", profile.Kill));
            if (enemies.Count == 1 && !adapter.EnemyHasCardsLeft()) parts.Add(("LastEnemy", profile.LastEnemy));
        }

        float element = adapter.ElementModifier(attacker.Data.Element, target.Data.Element);
        parts.Add(("Element", profile.ElementAdvantage * (element - 1f) * 2f));

        parts.Add(("Tile", profile.TileBonus * (adapter.TileModifier(attacker, fromTile) - 1f) / 0.3f));
        return kills ? target : null;
    }

    private static void AddDefence(List<(string, float)> parts, Creature actor, CreatureData data, Tile from, Tile to, Creature killed, BattleAdapter adapter, AIProfile profile, ThreatMap threat)
    {
        float health = actor != null ? actor.CurrentHealth : data.MaxHealth;
        if (health <= 0f) return;

        float dangerAfter = threat.DamageAt(to, data, killed);
        float dangerBefore = from != null ? threat.DamageAt(from, data) : 0f;
        float shareAfter = Mathf.Min(1f, dangerAfter / health);
        float shareBefore = Mathf.Min(1f, dangerBefore / health);

        parts.Add(("Danger", -profile.Danger * (shareAfter - shareBefore)));

        float dies = (from != null && dangerBefore >= health ? 1f : 0f) - (dangerAfter >= health ? 1f : 0f);
        if (killed != null && dies < 0f) dies = 0f;
        parts.Add(("WouldDie", profile.WouldDie * dies));

        if (actor != null && data.MaxHealth > 0f)
        {
            float hurt = 1f - Mathf.Clamp01(health / data.MaxHealth);
            parts.Add(("Retreat", profile.Retreat * hurt * (shareBefore - shareAfter)));
        }

        float alliesBefore = from != null ? AlliesNextTo(from, actor, adapter) : 0f;
        parts.Add(("Together", profile.StayTogether * (AlliesNextTo(to, actor, adapter) - alliesBefore) / 2f));
    }

    private static float AlliesNextTo(Tile tile, Creature actor, BattleAdapter adapter)
    {
        int count = 0;
        foreach (Creature ally in adapter.MyCreatures)
        {
            if (ally == null || ally == actor || ally.OccupyingTile == null) continue;
            Vector2Int gap = ally.OccupyingTile.TileIndex - tile.TileIndex;
            if (Mathf.Abs(gap.x) <= 1 && Mathf.Abs(gap.y) <= 1) count++;
        }
        return Mathf.Min(2, count);
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

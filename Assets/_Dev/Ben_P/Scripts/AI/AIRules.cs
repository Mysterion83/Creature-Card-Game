using System.Collections.Generic;
using UnityEngine;

/*
 * AIRules is the Week 2 brain. It only decides, it never touches the board:
 * AIController asks it what to do and then does it through BattleAdapter.
 * Each creature uses the first rule that fits: Kill, Focus, Attack, Advance, Wait.
 * Distance is counted in grid steps (x difference plus y difference).
 */

public enum RuleType { Kill, Focus, Attack, Advance, Wait }

public struct RuleChoice
{
    public RuleType Rule;
    public Tile Target;
}

public static class AIRules
{
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

    public static Tile ChooseSummonTile(List<Tile> freeTiles, List<Creature> enemies, Vector2Int boardCentre)
    {
        Tile best = null;
        int bestDistance = int.MaxValue;
        foreach (Tile tile in freeTiles)
        {
            int distance = enemies.Count > 0
                ? DistanceToNearest(tile.TileIndex, enemies)
                : Distance(tile.TileIndex, boardCentre);
            if (distance < bestDistance)
            {
                best = tile;
                bestDistance = distance;
            }
        }
        return best;
    }

    public static RuleChoice ChooseAction(Creature creature, BattleAdapter adapter, List<Creature> enemies)
    {
        RuleChoice attack = ChooseAttack(creature, adapter);
        if (attack.Rule != RuleType.Wait) return attack;

        Tile step = ChooseAdvanceTile(creature, adapter, enemies);
        if (step != null) return new RuleChoice { Rule = RuleType.Advance, Target = step };

        return new RuleChoice { Rule = RuleType.Wait };
    }

    public static RuleChoice ChooseAttack(Creature attacker, BattleAdapter adapter)
    {
        Tile kill = null, focus = null, strongest = null;
        float strongestDamage = 0f;

        foreach (Tile tile in adapter.AttackTiles(attacker))
        {
            Creature enemy = tile.Occupant;
            float damage = adapter.PredictDamage(attacker, enemy);
            if (damage <= 0f) continue;

            if (damage >= enemy.CurrentHealth && (kill == null || enemy.CurrentHealth < kill.Occupant.CurrentHealth))
                kill = tile;

            if (enemy.CurrentHealth < enemy.Data.MaxHealth && (focus == null || enemy.CurrentHealth < focus.Occupant.CurrentHealth))
                focus = tile;

            if (damage > strongestDamage)
            {
                strongest = tile;
                strongestDamage = damage;
            }
        }

        if (kill != null) return new RuleChoice { Rule = RuleType.Kill, Target = kill };
        if (focus != null) return new RuleChoice { Rule = RuleType.Focus, Target = focus };
        if (strongest != null) return new RuleChoice { Rule = RuleType.Attack, Target = strongest };
        return new RuleChoice { Rule = RuleType.Wait };
    }

    private static Tile ChooseAdvanceTile(Creature creature, BattleAdapter adapter, List<Creature> enemies)
    {
        int bestDistance = DistanceToNearest(creature.OccupyingTile.TileIndex, enemies);
        Tile best = null;

        foreach (Tile tile in adapter.MoveTiles(creature))
        {
            int distance = DistanceToNearest(tile.TileIndex, enemies);
            if (distance < bestDistance)
            {
                best = tile;
                bestDistance = distance;
            }
        }
        return best;
    }
}

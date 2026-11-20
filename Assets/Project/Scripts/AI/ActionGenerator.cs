using System.Collections.Generic;

/*
 * ActionGenerator lists every legal action for the current phase. In the summon
 * phase that is the next card on every free spawn tile. In the action phase it is
 * every attack, every move, and every move-then-attack each creature can make,
 * as long as there is energy for it.
 */

public static class ActionGenerator
{
    public static List<AIAction> Summons(BattleAdapter adapter)
    {
        var actions = new List<AIAction>();
        if (!adapter.CanSummon) return actions;

        foreach (Tile tile in adapter.FreeTiles())
            actions.Add(new AIAction { Type = AIActionType.Summon, Card = adapter.NextCard, Tile = tile });
        return actions;
    }

    public static List<AIAction> Actions(BattleAdapter adapter, HashSet<Creature> moved, HashSet<Creature> attacked)
    {
        var actions = new List<AIAction>();
        float cost = adapter.ActionCost;
        if (adapter.Energy <= 0f || adapter.Energy < cost) return actions;
        bool canDoTwo = adapter.Energy >= cost * 2f;

        foreach (Creature creature in adapter.MyCreatures)
        {
            if (creature == null) continue;
            bool canMove = !moved.Contains(creature);
            bool canAttack = !attacked.Contains(creature);

            if (canAttack)
                foreach (Tile target in adapter.AttackTiles(creature))
                    actions.Add(new AIAction { Type = AIActionType.Attack, Actor = creature, Tile = target });

            if (!canMove) continue;

            foreach (Tile move in adapter.MoveTiles(creature))
            {
                actions.Add(new AIAction { Type = AIActionType.Move, Actor = creature, Tile = move });
                if (!canDoTwo || !canAttack) continue;

                foreach (Tile target in adapter.AttackTilesFrom(creature, move.TileIndex))
                    actions.Add(new AIAction { Type = AIActionType.MoveThenAttack, Actor = creature, Tile = move, AttackTarget = target });
            }
        }
        return actions;
    }
}

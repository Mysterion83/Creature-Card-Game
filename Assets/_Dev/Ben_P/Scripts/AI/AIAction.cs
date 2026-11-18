/*
 * AIAction is one thing the AI could do this step: summon a card on a tile, move a
 * creature, attack with it, or move and then attack. ActionGenerator makes the list,
 * ActionScorer fills in Score, and AIController performs the highest one.
 */

public enum AIActionType { Summon, Move, Attack, MoveThenAttack }

public class AIAction
{
    public AIActionType Type;
    public Creature Actor;
    public CreatureData Card;
    public Tile Tile;
    public Tile AttackTarget;
    public float Score;

    public override string ToString()
    {
        switch (Type)
        {
            case AIActionType.Summon:
                return $"Summon {Card.CreatureName} at {Tile.TileIndex}";
            case AIActionType.Move:
                return $"Move {Actor.Data.CreatureName} {Actor.OccupyingTile.TileIndex} -> {Tile.TileIndex}";
            case AIActionType.Attack:
                return $"Attack {Actor.Data.CreatureName} -> {TargetName(Tile)} at {Tile.TileIndex}";
            default:
                return $"Move {Actor.Data.CreatureName} {Actor.OccupyingTile.TileIndex} -> {Tile.TileIndex} then attack {TargetName(AttackTarget)} at {AttackTarget.TileIndex}";
        }
    }

    private static string TargetName(Tile tile) => tile != null && tile.Occupant != null ? tile.Occupant.Data.CreatureName : "?";
}

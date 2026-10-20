using UnityEngine;

public class Creature : MonoBehaviour
{
    public PlayerData Owner { get; private set; }

    public CreatureData Data;

    public float CurrentHealth;
    public float Experience;



    public AttackResult TakeDamage(CreatureData attacker, TempTileData attackerTile)
    {
        AttackResult result = new AttackResult();
        float damage = Mathf.Max(0, attacker.Attack * attackerTile.AttackerTileModifier - Data.Defense);


        CurrentHealth -= damage;
        result.DamageDealt = damage;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            result.IsDead = true;
        }
        return result;
    }


    public Vector2Int[] ShowValidMovement()
    {
        return Data.MovementPattern.ValidPositions;
    }

    public bool AttemptMove(Vector2Int targetPosition, Vector2Int currentPosition)
    {
        if (Data.MovementPattern.ValidatePosition(targetPosition, currentPosition))
        {
            transform.position = new Vector3(targetPosition.x, targetPosition.y, 0);
            return true;
        }
        return false;
    }

    public Creature(CreatureData data, PlayerData owner)
    {
        Data = data;
        Owner = owner;
        CurrentHealth = data.MaxHealth;
        Experience = 0;
    }
}


//temp class
public class TempTileData 
{
    public Element TileElement;
    public int TileEffectStrength;
    public float AttackerTileModifier;
    public Creature Occupant;
}
using System;
using UnityEngine;

public class Creature : MonoBehaviour
{
    public PlayerData Owner { get; private set; }

    public CreatureData Data;

    public float CurrentHealth;
    public float Experience;

    public event Action<Creature> OnCreatureDeath;
    public event Action<Creature, float> OnCreatureDamaged;



    public void TakeDamage(CreatureData attacker, TileData attackerTile)
    {
        float damage = Mathf.Max(0, attacker.Attack * attackerTile.AttackerTileModifier - Data.Defense);

        CurrentHealth -= damage;
        OnCreatureDeath.Invoke(this);
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            OnCreatureDeath.Invoke(this);
        }
    }

    public Vector2Int[] ShowValidMovement()
    {
        return Data.MovementPattern.ValidPositions;
    }

    [Obsolete("Outdated")]
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
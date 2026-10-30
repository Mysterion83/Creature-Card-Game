using System;
using UnityEngine;

public class Creature : MonoBehaviour
{
    public PlayerData Owner { get; set; }

    public CreatureData Data;

    public Tile OccupyingTile;

    public float CurrentHealth;
    public float Experience;

    public event Action<Creature> OnCreatureDeath;
    public event Action<Creature, float> OnCreatureDamaged;

    public void TakeDamage(CreatureData attacker, TileData attackerTile)
    {
        float damage = Mathf.Max(0, attacker.Attack * attackerTile.AttackerTileModifier - Data.Defense);

        CurrentHealth -= damage;
        OnCreatureDamaged?.Invoke(this, damage);
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            OnCreatureDeath?.Invoke(this);
            Destroy(gameObject);
        }
    }

    public Vector2Int[] ShowValidMovement()
    {
        return Data.MovementPattern.ValidPositions;
    }
    public Vector2Int[] ShowValidAttacks()
    {
        return Data.AttackPattern.ValidPositions;
    }

    public Creature(CreatureData data, PlayerData owner)
    {
        Data = data;
        Owner = owner;
        CurrentHealth = data.MaxHealth;
        Experience = 0;
    }
}
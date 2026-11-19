using System;
using System.Collections;
using UnityEngine;

public class Creature : MonoBehaviour
{
    public Player Owner { get; set; }

    public CreatureData Data;

    public Tile OccupyingTile;

    public float CurrentHealth;
    public float Experience;

    public Vector3 TargetPosition;
    
    public bool IsAttacking { get; private set; }
    public bool HasMoved { get; private set; }
    public bool HasAttacked { get; private set; }
    
    public event Action<Creature> OnCreatureDeath;
    public event Action<Creature, AttackResult> OnCreatureDamaged;
    public event Action<Creature, Creature> OnAttackStarted;
    public event Action<Creature, Creature> OnAttackFired;

    public static event Action<AttackResult> OnAnyAttackImpact;
    public static event Action<Creature> OnAnyAttackFinished;
    
    public void Awake()
    {
        TargetPosition = transform.position;
    }

    public void Update()
    {
        transform.position = Vector3.Lerp(transform.position, TargetPosition, Time.deltaTime * 5f);

        Vector3 movementDirection = TargetPosition - transform.position;
        movementDirection.y = 0f;

        if (movementDirection.sqrMagnitude <= 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(movementDirection);

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    public void Attack(Creature target)
    {
        if (IsAttacking || !target) return;
        StartCoroutine(AttackRoutine(target));
        HasAttacked = true;
    }

    private IEnumerator AttackRoutine(Creature target)
    {
        IsAttacking = true;
        
        // Windup
        OnAttackStarted?.Invoke(this, target);
        yield return new WaitForSeconds(Data.WindupDuration);
        
        // Attack Start
        if (target && target.CurrentHealth > 0)
        {
            OnAttackFired?.Invoke(this, target);
            yield return new WaitForSeconds(Data.AttackDuration);
        }
        
        // Attack Impact
        if (target && target.CurrentHealth > 0)
        {
            AttackResult result = target.TakeDamage(this, OccupyingTile.Data);
            OnAnyAttackImpact?.Invoke(result);
        }
        
        // Recovery
        yield return new WaitForSeconds(Data.RecoveryDuration);

        IsAttacking = false;
        OnAnyAttackFinished?.Invoke(this);
    }
    
    public AttackResult TakeDamage(Creature attacker, TileData attackerTile)
    {
        float damage = CalculateDamage(attacker, this, attackerTile);
        
        CurrentHealth -= damage;
        bool isDead = CurrentHealth <= 0;

        AttackResult result = new()
        {
            Attacker = attacker,
            Defender = this,
            DamageDealt = damage,
            DamageType = DamageType.Normal,
            IsDead = isDead
        };

        OnCreatureDamaged?.Invoke(this, result);
        
        if (isDead)
        {
            CurrentHealth = 0;
            OnCreatureDeath?.Invoke(this);
            Destroy(gameObject);
        }
        
        return result;
    }

    public Vector2Int[] ShowValidMovement()
    {
        return Data.MovementPattern.ValidPositions;
    }
    public Vector2Int[] ShowValidAttacks()
    {
        return Data.AttackPattern.ValidPositions;
    }

    public static float CalculateDamage(Creature attacker, Creature defender, TileData attackerTile)
    {
        float elementModifier = attacker.Data.Element.ElementModifiers.ContainsKey(defender.Data.Element) ? attacker.Data.Element.ElementModifiers[defender.Data.Element] : 1f;
        return Mathf.Max(0, (attacker.Data.Attack * attackerTile.AttackerTileModifier - defender.Data.Defense) * elementModifier);
    }

    public void ResetTurnActions()
    {
        HasMoved = false;
        HasAttacked = false;
    }

    public void MarkMoved()
    {
        HasMoved = true;
    }
    
    public Creature(CreatureData data, Player owner)
    {
        Data = data;
        Owner = owner;
        CurrentHealth = data.MaxHealth;
        Experience = 0;
    }
}
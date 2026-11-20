using System;
using System.Collections;
using UnityEngine;

public class Creature : MonoBehaviour
{
    private const float NORMAL_DAMAGE_MODIFIER = 1f;
    private const float NORMAL_TILE_MODIFIER = 1f;
    private const float MOVEMENT_ANIM_DISTANCE_THRESHOLD = 0.001f;

    public Player Owner { get; set; }

    public CreatureData Data;

    public Tile OccupyingTile;

    public float CurrentHealth;
    public float Experience;

    /// <summary>
    /// Position the creature should be animated to move to
    /// </summary>
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
        //Lerp to target position
        transform.position = Vector3.Lerp(transform.position, TargetPosition, Time.deltaTime * 5f);

        Vector3 movementDirection = TargetPosition - transform.position;
        movementDirection.y = 0f;

        //stop changing the rotation when below a certain distance - causes rotation to reset if not
        if (movementDirection.sqrMagnitude <= MOVEMENT_ANIM_DISTANCE_THRESHOLD) return;

        //look at movement direction
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

    /// <summary>
    /// Deals Damage to the Creature with the given attacker
    /// </summary>
    /// <param name="attacker">The Creature dealing damage</param>
    /// <param name="attackerTile">The Tile the Attacker is on</param>
    /// <returns>A Result of the attack on the creature</returns>
    public AttackResult TakeDamage(Creature attacker, TileData attackerTile)
    {
        (float damage, DamageType damageType) = CalculateDamage(attacker, this, attackerTile);
        
        CurrentHealth -= damage;
        bool isDead = CurrentHealth <= 0;

        AttackResult result = new()
        {
            Attacker = attacker,
            Defender = this,
            DamageDealt = damage,
            DamageType = damageType,
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

    /// <summary>
    /// Simulates a combat move with the provided parameters
    /// </summary>
    /// <param name="attacker">The Creature dealing damage</param>
    /// <param name="defender">The Creature Being dealt Damage</param>
    /// <param name="attackerTile">The Tile the Attacker is on</param>
    /// <returns>A struct with damage dealt and type of damage</returns>
    public static (float Damage, DamageType DamageType) CalculateDamage(Creature attacker, Creature defender, TileData attackerTile)
    {
        float elementModifier = attacker.Data.Element.ElementModifiers.ContainsKey(defender.Data.Element) ? attacker.Data.Element.ElementModifiers[defender.Data.Element] : NORMAL_DAMAGE_MODIFIER;
        DamageType damageType = elementModifier > NORMAL_DAMAGE_MODIFIER ? DamageType.Advantage : (elementModifier < NORMAL_DAMAGE_MODIFIER ? DamageType.Disadvantage : DamageType.Normal);

        //Calculate tile modifier advantage/disadvantage
        float tileModifier = NORMAL_TILE_MODIFIER;
        if (attacker.Data.Element == attackerTile.Element)
        {
            tileModifier += attackerTile.AttackerTileModifier;
        }
        else
        {
            tileModifier -= attackerTile.AttackerTileModifier;
        }


        return (Mathf.Max(0, (attacker.Data.Attack * tileModifier - defender.Data.Defense) * elementModifier), damageType);
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
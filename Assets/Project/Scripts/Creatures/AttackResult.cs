public struct AttackResult
{
    /// <summary>
    /// The creature attacking the defender
    /// </summary>
    public Creature Attacker;
    /// <summary>
    /// Gets or sets the defending creature.
    /// </summary>
    public Creature Defender;
    /// <summary>
    /// The amount of damage dealt.
    /// </summary>
    public float DamageDealt;
    /// <summary>
    /// Type of Damage
    /// </summary>
    public DamageType DamageType;
    /// <summary>
    /// Whether or not the defender is dead after the attack.
    /// </summary>
    public bool IsDead;
}

public enum DamageType
{
    /// <summary>
    /// Dealt Regular Damage
    /// </summary>
    Normal,
    /// <summary>
    /// Dealt Higher than Normal Damage
    /// </summary>
    Advantage,
    /// <summary>
    /// Dealt Lower than Normal Damage
    /// </summary>
    Disadvantage
}
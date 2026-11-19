public struct AttackResult
{
    public Creature Attacker;
    public Creature Defender;
    public float DamageDealt;
    public DamageType DamageType;
    public bool IsDead;
}

public enum DamageType
{
    Normal,
    Advantage,
    Disadvantage
}
using UnityEngine;

/*
 * AIProfile holds the weights ActionScorer uses, so the AI can be tuned in the
 * Inspector without touching code. Higher weight = the AI cares more about that.
 */

[CreateAssetMenu(fileName = "AIProfile", menuName = "Ben_P/AI Profile")]
public class AIProfile : ScriptableObject
{
    [Header("Weights")]
    public float LastEnemy = 1000f;
    public float Kill = 30f;
    public float Attack = 8f;
    public float Damage = 20f;
    public float ElementAdvantage = 8f;
    public float TileBonus = 6f;
    public float Advance = 4f;
    public float Summon = 10f;

    [Header("Defence")]
    public float Danger = 8f;
    public float WouldDie = 25f;
    public float Retreat = 6f;
    public float StayTogether = 3f;

    [Header("Zones")]
    public float Zone = 15f;
    public float ZoneApproach = 2f;
    public float ZoneGuard = 5f;

    [Header("Turn")]
    public float EndTurnThreshold = 1f;
    public int MaxActionsPerPhase = 8;

    [Header("Debug")]
    public bool LogDecisions = true;
}

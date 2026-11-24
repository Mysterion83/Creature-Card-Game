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
    public float Damage = 20f;
    public float ElementAdvantage = 8f;
    public float TileBonus = 6f;
    public float Advance = 4f;
    public float Summon = 10f;

    [Header("Defence")]
    public float Danger = 15f;
    public float WouldDie = 40f;
    public float Retreat = 10f;
    public float StayTogether = 3f;

    [Header("Turn")]
    public float EndTurnThreshold = 1f;
    public int MaxActionsPerPhase = 8;

    [Header("Debug")]
    public bool LogDecisions = true;
}

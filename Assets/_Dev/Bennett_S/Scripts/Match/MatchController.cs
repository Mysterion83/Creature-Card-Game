using UnityEngine;

public class MatchController : MonoBehaviour
{
    public MatchState state = MatchState.Summon;
    public int turnsPlayed = 0;
    
    [Header("Turn Settings"), SerializeField]
    private float _maxTurnTime = 30f;
    
    private void Awake()
    {
        state = MatchState.Summon;
        
        
    }

    private void Update()
    {
        
    }
}

public enum MatchState
{
    Summon,
    Move,
    Battle,
    EndTurn
}
using System;
using UnityEngine;

public class MatchController : MonoBehaviour
{
    [Header("Match Settings"), SerializeField]
    private bool _beginOnStart = true;
    
    [Header("Turn Settings"), SerializeField]
    private float _maxPhaseTime = 30f;
    
    [Header("Debug"), SerializeField]
    private bool _enableTurnDebugging;

    private int _turnsPlayed;
    private int _currentPlayerIndex;
    private float _phaseTimer;

    public bool IsMatchActive { get; private set; }
    public Player CurrentPlayer {get; private set;}
    public MatchState CurrentState { get; private set; } = MatchState.Setup;

    public static event Action<MatchState> OnMatchStateChanged;
    public static event Action<Player, MatchState, float> OnPhaseStarted; // Sends active player and time limit
    public static event Action<Player> OnTurnEnded;
    public static event Action OnMatchStarted;
    public static event Action<Player> OnMatchEnded;

    private void OnEnable()
    {
        PlayerEvents.OnPlayerDefeated += HandlePlayerDefeated;
    }
    private void OnDisable()
    {
        PlayerEvents.OnPlayerDefeated -= HandlePlayerDefeated;
    }

    private void Start()
    {
        if (_beginOnStart) StartMatch();
    }
    
    private void Update()
    {
        if (!IsMatchActive) return;
        
        _phaseTimer -= Time.deltaTime;
        if (!(_phaseTimer <= 0f)) return;
        if (_enableTurnDebugging) Debug.Log($"MatchController: {CurrentPlayer.Data.Name}'s {CurrentState} phase timed out!");
        RequestNextPhase();
    }

    /// <summary>
    /// Start a new match
    /// </summary>
    public void StartMatch()
    {
        if (PlayerRegistry.PlayerCount == 0)
        {
            Debug.LogError("MatchController Error: Match could not start as no players have been registered!"); 
            return;
        }
        
        OnMatchStarted?.Invoke();
        
        IsMatchActive = true;
        _turnsPlayed = 0;
        
        _currentPlayerIndex = UnityEngine.Random.Range(0, PlayerRegistry.PlayerCount);
        CurrentPlayer = PlayerRegistry.MasterList[_currentPlayerIndex];

        Debug.Log(PlayerRegistry.PlayerCount);
        
        StartNextTurn();
    }
    
    private void StartNextTurn()
    {
        if (_turnsPlayed > 0) _currentPlayerIndex = (_currentPlayerIndex + 1) % PlayerRegistry.PlayerCount;
        CurrentPlayer = PlayerRegistry.MasterList[_currentPlayerIndex];
        
        _turnsPlayed++;
        CurrentPlayer.StartTurn();
        EnterPhase(MatchState.TurnSummon);
    }

    private void EnterPhase(MatchState newPhase)
    {
        CurrentState = newPhase;
        OnMatchStateChanged?.Invoke(CurrentState);

        switch (CurrentState)
        {
            case MatchState.TurnSummon when !CurrentPlayer.CanSummon():
            {
                if (_enableTurnDebugging) Debug.Log($"MatchController: Skipping Summon Phase for {CurrentPlayer.Data.Name} (No energy/cards).");
                EnterPhase(MatchState.TurnAction);
                return;
            }
            case MatchState.TurnAction when !CurrentPlayer.CanMoveOrAttack():
            {
                if (_enableTurnDebugging) Debug.Log($"MatchController: Skipping Action Phase for {CurrentPlayer.Data.Name} (No energy/units).");
                RequestEndTurn();
                return;
            }
            default:
                _phaseTimer = _maxPhaseTime;
                OnPhaseStarted?.Invoke(CurrentPlayer, CurrentState, _phaseTimer);
                break;
        }
    }

    /// <summary>
    /// Progress to the next phase of an active turn, called internally or through UI
    /// </summary>
    public void RequestNextPhase()
    {
        if (!IsMatchActive) return;

        switch (CurrentState)
        {
            case MatchState.TurnSummon:
                EnterPhase(MatchState.TurnAction);
                break;
            case MatchState.TurnAction:
                RequestEndTurn();
                break;
            default:
                return;
        }
    }
    
    /// <summary>
    /// Force end a turn, should be called through UI or changed in future to subscribe
    /// </summary>
    public void RequestEndTurn()
    {
        if (!IsMatchActive || !CurrentPlayer) return;

        CurrentPlayer.EndTurn();
        OnTurnEnded?.Invoke(CurrentPlayer);
        
        StartNextTurn();
    }

    private void ChangeMatchState(MatchState state)
    {
        if (CurrentState == state) return;
        
        CurrentState = state;
        OnMatchStateChanged?.Invoke(CurrentState);
    }
    
    private void HandlePlayerDefeated(Player defeatedPlayer)
    {
        int aliveCount = 0;
        Player potentialWinner = null;

        foreach (Player player in PlayerRegistry.MasterList)
        {
            if (player.IsDefeated) continue;
            
            aliveCount++;
            potentialWinner = player;
        }

        if (aliveCount <= 1)
        {
            CompleteMatch(potentialWinner);
        }
    }
    
    private void CompleteMatch(Player winner)
    {
        IsMatchActive = false;
        ChangeMatchState(MatchState.MatchOver);
        OnMatchEnded?.Invoke(winner);
        
        if (!_enableTurnDebugging) return;
        Debug.Log($"MatchController: Player {winner.Data.Name} was Defeated.");
    }
    
    private void OnDestroy()
    {
        PlayerRegistry.Clear();
    }
}

public enum MatchState
{
    Setup,
    TurnSummon,      // Current player's summoning window
    TurnAction,      // Current player's move/attack window
    TurnEnded,
    MatchOver
}
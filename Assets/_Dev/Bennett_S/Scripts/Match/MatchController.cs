using System;
using UnityEngine;

public class MatchController : MonoBehaviour
{
    [Header("Match Settings")]
    private bool _beginOnStart = true;
    
    [Header("Turn Settings"), SerializeField]
    private float _maxTurnTime = 30f;
    
    [Header("Debug"), SerializeField]
    private bool _enableTurnDebugging;

    private int _turnsPlayed;

    private int _startingPlayerIndex;
    private int _currentPlayerIndex;
    private int _remainingPlayers;
    
    private float _turnTimer;

    public bool IsMatchActive { get; private set; }
    public Player CurrentPlayer { get; private set; }

    public MatchState CurrentState { get; private set; } = MatchState.Summon;

    public static event Action<MatchState> OnMatchStateChanged;
    public static event Action<Player, float> OnTurnStarted; // Sends active player and time limit
    public static event Action<Player> OnTurnEnded;
    public static event Action OnMatchStarted;
    public static event Action<Player> OnMatchEnded;

    private void OnEnable()
    {
        PlayerEvents.OnPlayerDefeated += HandlePlayerDefeated;
        PlayerEvents.OnPlayerSummonStageCompleted += HandlePlayerSummonStageCompleted;
    }
    private void OnDisable()
    {
        PlayerEvents.OnPlayerDefeated -= HandlePlayerDefeated;
        PlayerEvents.OnPlayerSummonStageCompleted -= HandlePlayerSummonStageCompleted;
    }

    private void Start()
    {
        if (_beginOnStart) StartMatch();
    }
    
    private void Update()
    {
        if (!IsMatchActive) return;
        
        if (CurrentState == MatchState.Summon) return;
        
        _turnTimer -= Time.deltaTime;
        if (!(_turnTimer <= 0f)) return;
            
        if (_enableTurnDebugging) Debug.Log($"MatchController: {PlayerRegistry.MasterList[_currentPlayerIndex].Data.Name}'s turn timed out!");
        RequestEndTurn();
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
        _remainingPlayers = PlayerRegistry.PlayerCount;
        
        ChangeMatchState(MatchState.Summon);
        
        _startingPlayerIndex = UnityEngine.Random.Range(0, PlayerRegistry.PlayerCount);
        _currentPlayerIndex = _startingPlayerIndex;

        ChangeTurn();
    }
    
    private void ChangeTurn()
    {
        _turnsPlayed++;
        
        if (_turnsPlayed > PlayerRegistry.PlayerCount)
        {
            ChangeMatchState(MatchState.Gameplay);
        }

        // Choose the next player index relative to the starting player
        _currentPlayerIndex = (_startingPlayerIndex + (_turnsPlayed - 1)) % PlayerRegistry.PlayerCount;
        CurrentPlayer = PlayerRegistry.MasterList[_currentPlayerIndex];
        
        _turnTimer = _maxTurnTime;
        OnTurnStarted?.Invoke(CurrentPlayer, _turnTimer);
        
        Debug.Log($"MatchController: Turn {_turnsPlayed} | State: {CurrentState} | Player: {CurrentPlayer.Data.Name} (Index: {_currentPlayerIndex})");
        CurrentPlayer.StartTurn();
    }

    /// <summary>
    /// Force end a turn, should be called through UI or changed in future to subscribe
    /// </summary>
    public void RequestEndTurn()
    {
        if (!IsMatchActive || CurrentPlayer == null) return;

        OnTurnEnded?.Invoke(CurrentPlayer);
        CurrentPlayer.EndTurn();
        ChangeTurn();
    }
    
    private void CompleteMatch()
    {
        IsMatchActive = false;
        Player winner = null;
        
        for (int i = 0; i < PlayerRegistry.PlayerCount; i++)
        {
            if (PlayerRegistry.MasterList[i].IsDefeated) continue;
            
            winner = PlayerRegistry.MasterList[i];
            OnMatchEnded?.Invoke(winner);
            break;
        }

        ChangeMatchState(MatchState.Ended);
        
        if (!_enableTurnDebugging) return;
        if (winner != null) Debug.Log($"MatchController: Player {winner.Data.Name} was Defeated.");
    }

    private void ChangeMatchState(MatchState state)
    {
        if (CurrentState == state) return;
        
        CurrentState = state;
        OnMatchStateChanged?.Invoke(CurrentState);
    }
    
    private void OnDestroy()
    {
        PlayerRegistry.Clear();
    }
    
    private void HandlePlayerDefeated(Player player)
    {
        if (!IsMatchActive) return;

        _remainingPlayers--;
        if (_enableTurnDebugging) Debug.Log($"MatchController: Player {player} was Defeated.");

        if (_remainingPlayers <= 1) CompleteMatch();
    }

    private void HandlePlayerSummonStageCompleted(Player player)
    {
        if (CurrentState != MatchState.Summon || CurrentPlayer != player) return;
        
        if (_enableTurnDebugging) Debug.Log($"MatchController: Player {player} has summoned all their creatures!");
        RequestEndTurn();
    }
}

public enum MatchState
{
    Summon,
    Gameplay,
    Ended
}
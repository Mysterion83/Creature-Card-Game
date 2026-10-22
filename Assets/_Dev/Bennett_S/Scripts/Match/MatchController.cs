using System;
using UnityEngine;

public class MatchController : MonoBehaviour
{
    [Header("Match Settings")]
    private bool _beginOnStart = true;
    
    [Header("Turn Settings"), SerializeField]
    private float _maxTurnTime = 30f;
    
    [Header("Debug"), SerializeField]
    private bool _enableTurnDebugging = false;
    
    private MatchState _state = MatchState.Summon;
    private int _turnsPlayed = 0;

    private IPlayer _currentPlayer;
    private int _startingPlayerIndex = 0;
    private int _currentPlayerIndex = 0;
    private int _remainingPlayers;
    
    private float _turnTimer = 0f;
    private bool _isSetupComplete = false;
    
    public bool IsMatchActive { get; private set; } = false;
    
    public static event Action<MatchState> OnMatchStateChanged;
    public static event Action<IPlayer, float> OnTurnStarted; // Sends active player and time limit
    public static event Action<IPlayer> OnTurnEnded;
    public static event Action<IPlayer> OnMatchEnded;

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
        
        if (_state == MatchState.Summon) return;
        
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
        
        IsMatchActive = true;
        _isSetupComplete = false;
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
        _currentPlayer = PlayerRegistry.MasterList[_currentPlayerIndex];
        
        _turnTimer = _maxTurnTime;
        OnTurnStarted?.Invoke(_currentPlayer, _turnTimer);
        
        Debug.Log($"MatchController: Turn {_turnsPlayed} | State: {_state} | Player: {_currentPlayer.Data.Name} (Index: {_currentPlayerIndex})");
        _currentPlayer.StartTurn();
    }

    /// <summary>
    /// Force end a turn, should be called through UI or changed in future to subscribe
    /// </summary>
    public void RequestEndTurn()
    {
        if (!IsMatchActive || _currentPlayer == null) return;

        OnTurnEnded?.Invoke(_currentPlayer);
        _currentPlayer.EndTurn();
        ChangeTurn();
    }
    
    private void CompleteMatch()
    {
        IsMatchActive = false;
        IPlayer winner = null;
        
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
        if (_state == state) return;
        
        _state = state;
        OnMatchStateChanged?.Invoke(_state);
    }
    
    private void OnDestroy()
    {
        PlayerRegistry.Clear();
    }
    
    private void HandlePlayerDefeated(IPlayer player)
    {
        if (!IsMatchActive) return;

        _remainingPlayers--;
        if (_enableTurnDebugging) Debug.Log($"MatchController: Player {player} was Defeated.");

        if (_remainingPlayers <= 1) CompleteMatch();
    }

    private void HandlePlayerSummonStageCompleted(IPlayer player)
    {
        if (_state != MatchState.Summon || _currentPlayer != player) return;
        
        if (_enableTurnDebugging) Debug.Log($"MatchController: Player {player} was Defeated.");
        RequestEndTurn();
    }
}

public enum MatchState
{
    Summon,
    Gameplay,
    Ended
}
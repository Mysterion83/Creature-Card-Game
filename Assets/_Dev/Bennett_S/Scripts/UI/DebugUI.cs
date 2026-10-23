using System;
using System.Text;
using TMPro;
using UnityEngine;

public class DebugUI : MonoBehaviour
{
    [Header("References"), SerializeField]
    private TextMeshProUGUI _debugText;

    private float _runTime;
    private MatchState _state; // Assumed default enum state
    private bool _started;
    
    private IPlayer _currentPlayer;
    private float _turnTimeElapsed;
    private IPlayer _winner;
    
    private Creature _currentPlayerCreature;
    
    private readonly StringBuilder _stringBuilder = new StringBuilder();
    
    private void OnEnable()
    {
        MatchController.OnTurnStarted += OnTurnStarted;
        MatchController.OnTurnEnded += OnTurnEnded;
        MatchController.OnMatchStarted += OnMatchStarted;
        MatchController.OnMatchStateChanged += OnMatchStateChanged;
        MatchController.OnMatchEnded += OnMatchEnded;

        BoardController.OnCreatureSelected += OnCreatureSelected;
    }
    
    private void OnDisable()
    {
        MatchController.OnTurnStarted -= OnTurnStarted;
        MatchController.OnTurnEnded -= OnTurnEnded;
        MatchController.OnMatchStarted -= OnMatchStarted;
        MatchController.OnMatchStateChanged -= OnMatchStateChanged;
        MatchController.OnMatchEnded -= OnMatchEnded;
        
        BoardController.OnCreatureSelected -= OnCreatureSelected;
    }
    
    private void Update()
    {
        _runTime += Time.deltaTime;
        
        if (_started && _currentPlayer != null)
        {
            _turnTimeElapsed += Time.deltaTime;
        }

        UpdateDebugDisplay();
    }

    private void UpdateDebugDisplay()
    {
        if (!_debugText) return;

        _stringBuilder.Clear();
        
        // Header
        _stringBuilder.AppendLine("<b>=== Debug UI ===</b>");
        // Stats
        _stringBuilder.AppendLine($"[Runtime]: {_runTime:F2}s");
        _stringBuilder.AppendLine($"[Match Status]: {(_started ? "<color=green>ACTIVE</color>" : "<color=red>WAITING</color>")}");
        _stringBuilder.AppendLine($"[Match State]: {_state}");
        
        // Player/Turn Stats
        _stringBuilder.AppendLine("\n<b>=== Turn Stats ===</b>");
        if (_started)
        {
            if (_currentPlayer != null)
            {
                _stringBuilder.AppendLine($"[Active Player]: {_currentPlayer.Data.Name}"); // Assumed IPlayer has a Name property
                _stringBuilder.AppendLine($"[Turn Timer]: {_turnTimeElapsed:F1}s");
            }
            else
            {
                _stringBuilder.AppendLine("[Active Player]: Transitioning...");
            }
        }
        else if (_winner != null)
        {
            _stringBuilder.AppendLine($"<color=yellow>[Match Over] Winner: {_winner.Data.Name}</color>");
        }

        _stringBuilder.AppendLine("\n<b>=== Creature Stats ===</b>");
        _stringBuilder.AppendLine($"[Selected Creature]: {(_currentPlayerCreature ? _currentPlayerCreature.name : "None")}");
        _stringBuilder.AppendLine($"[Health]: {_currentPlayerCreature?.CurrentHealth.ToString() ?? "N/A"}");
        _stringBuilder.AppendLine($"[Position]: {_currentPlayerCreature?.OccupyingTile?.TileIndex.ToString() ?? "N/A"}");

        _debugText.text = _stringBuilder.ToString();
    }
    
    private void OnTurnStarted(IPlayer player, float time)
    {
        _currentPlayer = player;
        _turnTimeElapsed = 0f;
    }
    
    private void OnTurnEnded(IPlayer player)
    {
        _currentPlayer = null;
        _currentPlayerCreature = null;
    }
    
    private void OnMatchStarted()
    {
        _started = true;
        _winner = null;
    }
    
    private void OnMatchStateChanged(MatchState state)
    {
        _state = state;
    }

    private void OnMatchEnded(IPlayer player)
    {
        _started = false;
        _currentPlayer = null;
        _winner = player;
    }
    
    private void OnCreatureSelected(Creature creature)
    {
        _currentPlayerCreature = creature;   
    }
}

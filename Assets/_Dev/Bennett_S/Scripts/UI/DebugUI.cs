using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Basic Text-Based UI for debugging match data
/// </summary>
public class DebugUI : MonoBehaviour
{
    [Header("References"), SerializeField]
    private TextMeshProUGUI _debugText;

    private float _runTime;
    private float _deltaTime;
    private float _fps;
    
    private MatchState _state;
    private bool _started;
    
    private Player _currentPlayer;
    private float _phaseTimeElapsed;
    private Player _winner;
    
    private Creature _currentPlayerCreature;
    
    private readonly StringBuilder _stringBuilder = new();
    
    private void OnEnable()
    {
        MatchController.OnPhaseStarted += OnPhaseStarted;
        MatchController.OnTurnEnded += OnTurnEnded;
        MatchController.OnMatchStarted += OnMatchStarted;
        MatchController.OnMatchStateChanged += OnMatchStateChanged;
        MatchController.OnMatchEnded += OnMatchEnded;

        BoardController.OnCreatureSelected += OnCreatureSelected;
    }
    
    private void OnDisable()
    {
        MatchController.OnPhaseStarted -= OnPhaseStarted;
        MatchController.OnTurnEnded -= OnTurnEnded;
        MatchController.OnMatchStarted -= OnMatchStarted;
        MatchController.OnMatchStateChanged -= OnMatchStateChanged;
        MatchController.OnMatchEnded -= OnMatchEnded;
        
        BoardController.OnCreatureSelected -= OnCreatureSelected;
    }
    
    private void Update()
    {
        // Runtime Counter
        _runTime += Time.deltaTime;
        
        // FPS Counter
        _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        _fps = 1f / _deltaTime;
        
        if (_started && _currentPlayer)
        {
            _phaseTimeElapsed += Time.deltaTime;
        }

        UpdateDebugDisplay();
    }

    private void UpdateDebugDisplay()
    {
        if (!_debugText) return;

        _stringBuilder.Clear();
        
        // Header
        _stringBuilder.AppendLine("<b>=== Debug UI ===</b>");
            
        // Game Stats
        _stringBuilder.AppendLine($"[Runtime]: {_runTime:F2}s");
        _stringBuilder.AppendLine($"[FPS]: {Mathf.Ceil(_fps).ToString()}");
            
        // Match Stats
        _stringBuilder.AppendLine($"[Match Status]: {(_started ? "<color=green>ACTIVE</color>" : "<color=red>WAITING</color>")}");
        _stringBuilder.AppendLine($"[Match State]: {_state}");
        
        // Player/Turn Stats
        _stringBuilder.AppendLine("\n<b>=== Turn Stats ===</b>");
        if (_started)
        {
            if (_currentPlayer)
            {
                EnergySystem energySystem = _currentPlayer?.EnergySystem;
                _stringBuilder.AppendLine($"[Active Player]: {_currentPlayer?.Data.Name}");
                if (energySystem) _stringBuilder.AppendLine($"[Energy]: {energySystem.Energy.CurrentAmount}/{energySystem.Energy.MaxAmount}");
                _stringBuilder.AppendLine($"[Phase Timer]: {_phaseTimeElapsed:F1}s");
            }
            else
            {
                _stringBuilder.AppendLine("[Active Player]: Transitioning...");
            }
        }
        else if (_winner)
        {
            _stringBuilder.AppendLine($"<color=yellow>[Match Over] Winner: {_winner.Data.Name}</color>");
        }

        _stringBuilder.AppendLine("\n<b>=== Creature Stats ===</b>");
        _stringBuilder.AppendLine($"[Selected Creature]: {(_currentPlayerCreature ? _currentPlayerCreature.name : "None")}");
        _stringBuilder.AppendLine($"[Health]: {_currentPlayerCreature?.CurrentHealth.ToString() ?? "N/A"}");
        _stringBuilder.AppendLine($"[Energy Cost]: {_currentPlayerCreature?.Data.EnergyCost.ToString() ?? "N/A"}");
        _stringBuilder.AppendLine($"[Position]: {_currentPlayerCreature?.OccupyingTile?.TileIndex.ToString() ?? "N/A"}");
        _stringBuilder.AppendLine($"[Movement Pattern]: {_currentPlayerCreature?.Data.MovementPattern.ToString() ?? "N/A"}");
        _stringBuilder.AppendLine($"[Attack Pattern]: {_currentPlayerCreature?.Data.AttackPattern.ToString() ?? "N/A"}");
        
        _debugText.text = _stringBuilder.ToString();
    }
    
    private void OnPhaseStarted(Player player, MatchState state, float time)
    {
        _currentPlayer = player;
        _phaseTimeElapsed = 0f;
    }
    
    private void OnTurnEnded(Player player)
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

    private void OnMatchEnded(Player player)
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

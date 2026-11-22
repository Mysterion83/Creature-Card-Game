using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Obsolete("Refactor into several classes")]
public class HUD : MonoBehaviour
{
    // Constants
    private const string PLAYER_COLOR_TAG = "<color=blue>";
    private const string ENEMY_COLOR_TAG = "<color=red>";
    private const string END_COLOR_TAG = "</color>";
    private const string ATTACK_COLOR_TAG = "<color=red>";
    private const string HP_COLOR_TAG = "<color=#FF81A9FF>";
    private const string DEF_COLOR_TAG = "<color=#00ffffff>";

    private const float PHASE_POPUP_DURATION = 2f;
    private const float NO_ENERGY_POPUP_DURATION = 2f;

    // Variables

    // Energy
    [SerializeField] private TextMeshProUGUI _energyCounter;
    [SerializeField] private TextMeshProUGUI _energyBonusText;
    [SerializeField] private GameObject _noEnergyPopup;
    // Objectives
    [SerializeField] private TextMeshProUGUI _objectiveCounter;
    // Turn Indictator
    [SerializeField] private TextMeshProUGUI _turnText;
    [SerializeField] private TextMeshProUGUI _turnTimer;
    // Phase
    [SerializeField] private Image _summoningPhase;
    [SerializeField] private Image _combatPhase;
    [SerializeField] private Color _defaultPhaseColour;
    [SerializeField] private Color _activePhaseColour;
    [SerializeField] private Image _phasePopup;
    [SerializeField] private TextMeshProUGUI _phasePopupText;
    [SerializeField] private TextMeshProUGUI _phasePopupTurnText;

    // Info Box
    [SerializeField] private GameObject _infoBox;
    [SerializeField] private TextMeshProUGUI _infoBoxText;

    // Combat Log
    [SerializeField] private GameObject _combatLog;
    [HideInInspector] private List<string> _combatLogValues = new List<string>();
    [HideInInspector] private List<string> _combatLogValuesRaw = new List<string>();
    [SerializeField] private string _combatLogRaw = string.Empty;
    [SerializeField] private TextMeshProUGUI _combatLogText;

    public void Start()
    {
        FindAnyObjectByType<BoardController>().OnTileSelected += SetInfoBoxData;
        _combatLogText.text = string.Empty;
    }

    /// <summary>
    /// Updates the phase type
    /// </summary>
    /// <param name="isSummoningPhase">is it the summoning phase or combat phase</param>
    public void UpdatePhase(bool isSummoningPhase)
    {
        if (isSummoningPhase)
        {
            _summoningPhase.color = _activePhaseColour;
            _combatPhase.color = _defaultPhaseColour;
            ShowPhasePopup("Summoning Phase");
        }
        else
        {
            _summoningPhase.color = _defaultPhaseColour;
            _combatPhase.color = _activePhaseColour;
            ShowPhasePopup("Combat Phase");
        }
    }
    /// <summary>
    /// Sets the info box data based on the given tile and optionally the selected creature's tile. If the tile is null, the info box is hidden. Otherwise, it displays information about the tile, its occupant (if any), and potential damage if a creature is selected.
    /// </summary>
    /// <param name="tile">The tile to display information for</param>
    /// <param name="selectedCreatureTile">The tile of the selected creature</param>
    public void SetInfoBoxData(Tile tile, Tile selectedCreatureTile = null)
    {
        if (tile == null)
        {
            _infoBox.SetActive(false);
            return;
        }
        _infoBox.SetActive(true);
        _infoBox.transform.position = Mouse.current.position.ReadValue();
        string tileInfo = $"<b>Tile:</b> {tile.Data.Name}\n" +
            $"<b>Element:</b> {tile.Data.Element?.Name}\n" +
            $"<b>Elemental Resonance:</b> {tile.Data.EffectStrength}\n";

        Creature occupant = tile.Occupant;

        if (occupant != null)
        {
            tileInfo += $"<b>Occupant:</b> {occupant.Data.CreatureName}\n" +
                $"<b>Owner:</b> {occupant.Owner.Data.IsAI}\n" +
                $"{HP_COLOR_TAG}<b>HP:</b>{END_COLOR_TAG} {FormatValue((int)occupant.CurrentHealth, (int)occupant.Data.MaxHealth)}\n" +
                $"{ATTACK_COLOR_TAG}<b>ATK:</b>{END_COLOR_TAG} {occupant.Data.Attack} {DEF_COLOR_TAG}<b>DEF:</b>{END_COLOR_TAG} {occupant.Data.Defense}\n";
            if (selectedCreatureTile != null && selectedCreatureTile.Occupant != null)
            {
                tileInfo += $"{ATTACK_COLOR_TAG}<b>Potential Damage:</b>{END_COLOR_TAG} {Creature.CalculateDamage(selectedCreatureTile.Occupant, occupant, selectedCreatureTile.Data).Damage : 0.00}\n";
            }
        }
        else
        {
            tileInfo += "<b>Occupant:</b> None\n";
        }
        _infoBoxText.text = tileInfo;
    }
    /// <summary>
    /// Updates the turn display with the current player's name.
    /// </summary>
    /// <param name="player">The player whose turn is being displayed.</param>
    public void UpdateTurn(PlayerData player)
    {
        _turnText.text = $"Turn: {player.Name}";
    }
    /// <summary>
    /// Updates the energy counter display.
    /// </summary>
    /// <param name="currentEnergy">The current energy value.</param>
    /// <param name="maxEnergy">The maximum energy value.</param>
    public void UpdateEnergyCounter(int currentEnergy, int maxEnergy)
    {
        _energyCounter.text = FormatValue(currentEnergy, maxEnergy);
    }
    /// <summary>
    /// Updates the energy bonus display.
    /// </summary>
    /// <param name="energyBonus">The energy bonus value.</param>
    public void UpdateEnergyBonus(int energyBonus)
    {
        _energyBonusText.text = energyBonus > 0 ? $"+{energyBonus}" : string.Empty;
    }
    /// <summary>
    /// Updates the objective counter display.
    /// </summary>
    /// <param name="currentObjectives">The current number of objectives.</param>
    /// <param name="maxObjectives">The maximum number of objectives.</param>
    public void UpdateObjectiveCounter(int currentObjectives, int maxObjectives)
    {
        _objectiveCounter.text = FormatValue(currentObjectives, maxObjectives);
    }
    /// <summary>
    /// Formats a value and its maximum as a slash-separated string.
    /// </summary>
    /// <param name="value">The current value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <returns>A string in the format "value/maxValue".</returns>
    static string FormatValue(int value, int maxValue)
    {
        return $"{value}/{maxValue}";
    }
    /// <summary>
    /// Displays a popup for the specified phase and current turn.
    /// </summary>
    /// <remarks>The popup closes automatically after the configured display duration.</remarks>
    /// <param name="phaseName">The name of the phase to display.</param>
    private void ShowPhasePopup(string phaseName)
    {
        CancelInvoke(nameof(ClosePopup));
        _phasePopupText.text = phaseName;
        _phasePopup.gameObject.SetActive(true);
        _phasePopupTurnText.text = _turnText.text;
        Invoke(nameof(ClosePopup), PHASE_POPUP_DURATION);
    }
    private void ClosePopup()
    {
        _phasePopup.gameObject.SetActive(false);
    }

    public void UpdateCombatLogValues(string text)
    {
        _combatLogValues.Add(text);
        UpdateCombatLogDisplay();
    }
    /// <summary>
    /// Logs an Attack Result
    /// </summary>
    /// <param name="attackResult">Attack Result to Insert into the combat log</param>
    public void UpdateCombatLogValues(AttackResult attackResult)
    {
        string logEntry = "";
        string rawLogEntry = "";

        if (!attackResult.Attacker.Owner.Data.IsAI) 
        {
            logEntry += $"{PLAYER_COLOR_TAG}{attackResult.Attacker.Data.CreatureName}{END_COLOR_TAG} ";
            rawLogEntry += $"{attackResult.Attacker.Data.CreatureName} ";
        }
        else 
        {
            logEntry += $"{ENEMY_COLOR_TAG}{attackResult.Attacker.Data.CreatureName}{END_COLOR_TAG} ";
            rawLogEntry += $"{attackResult.Attacker.Data.CreatureName} ";
        }

        if (attackResult.IsDead) 
        {
            logEntry += "killed ";
            rawLogEntry += "killed ";
        }
        else 
        {
            logEntry += "attacked ";
            rawLogEntry += "attacked ";
        }

        if (!attackResult.Defender.Owner.Data.IsAI) {
            logEntry += $"{PLAYER_COLOR_TAG}{attackResult.Defender.Data.CreatureName}{END_COLOR_TAG} ";
            rawLogEntry += $"{attackResult.Defender.Data.CreatureName} ";
        }
        else {
            logEntry += $"{ENEMY_COLOR_TAG}{attackResult.Defender.Data.CreatureName}{END_COLOR_TAG} ";
            rawLogEntry += $"{attackResult.Defender.Data.CreatureName} ";
        }

        if (attackResult.DamageDealt > 0 && !attackResult.IsDead) 
        {
            logEntry += $"for {attackResult.DamageDealt} damage.";
            rawLogEntry += $"for {attackResult.DamageDealt} damage.";
        }
        else if (attackResult.DamageDealt == 0 && !attackResult.IsDead) 
        {
            logEntry += "but it was ineffective.";
            rawLogEntry += "but it was ineffective.";
        }

        _combatLogValues.Add(logEntry);
        _combatLogValuesRaw.Add(rawLogEntry);
        UpdateCombatLogDisplay();
    }
    /// <summary>
    /// Regenerates the Combat Log String
    /// </summary>
    private void UpdateCombatLogDisplay()
    {
        _combatLogText.text = string.Empty;
        foreach (string feedLine in _combatLogValues)
        {
            _combatLogText.text += feedLine + "\n";
        }
        _combatLogRaw = string.Empty;
        foreach (string feedLine in _combatLogValuesRaw)
        {
            _combatLogRaw += feedLine + "\n";
        }
    }
    /// <summary>
    /// Shows a popup for a set duration
    /// </summary>
    public void ShowNoEnergyPopup()
    {
        CancelInvoke(nameof(HideNoEnergyPopup));
        _noEnergyPopup.SetActive(true);
        Invoke(nameof(HideNoEnergyPopup), NO_ENERGY_POPUP_DURATION);
    }

    private void HideNoEnergyPopup()
    {
        _noEnergyPopup.SetActive(false);
    }
    /// <summary>
    /// Updates the turn timer display with the remaining time.
    /// </summary>
    /// <remarks>Displays the time in minutes and seconds using the format "MM:SS".</remarks>
    /// <param name="timeRemaining">The remaining turn time in seconds.</param>
    public void UpdateTurnTimer(float timeRemaining)
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        _turnTimer.text = $"{minutes:00}:{seconds:00}";
    }

    public void ToggleCombatLog()
    {
        _combatLog.SetActive(!_combatLog.activeSelf);
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(HUD))]
public class HUDEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        HUD hud = (HUD)target;
        if (GUILayout.Button("Set Summoning Phase"))
        {
            hud.UpdatePhase(true);
        }
        if (GUILayout.Button("Set Combat Phase"))
        {
            hud.UpdatePhase(false);
        }
        if (GUILayout.Button("Add Combat Log Entry"))
        {
            hud.UpdateCombatLogValues("Test combat log entry.");
        }
    }
}

#endif
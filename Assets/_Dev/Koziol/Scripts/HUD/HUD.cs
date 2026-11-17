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
    private const string PLAYER_COLOR_TAG = "<color=blue>";
    private const string ENEMY_COLOR_TAG = "<color=red>";
    private const string END_COLOR_TAG = "</color>";
    private const string ATTACK_COLOR_TAG = "<color=red>";
    private const string HP_COLOR_TAG = "<color=#FF81A9FF>";
    private const string DEF_COLOR_TAG = "<color=#00ffffff>";

    private const float PHASE_POPUP_DURATION = 2f;
    private const float NO_ENERGY_POPUP_DURATION = 2f;


    [SerializeField] private TextMeshProUGUI _energyCounter;
    [SerializeField] private TextMeshProUGUI _energyBonusText;
    [SerializeField] private TextMeshProUGUI _objectiveCounter;
    [SerializeField] private GameObject _noEnergyPopup;
    [SerializeField] private TextMeshProUGUI _turnText;
    [SerializeField] private TextMeshProUGUI _turnTimer;
    [SerializeField] private Image _summoningPhase;
    [SerializeField] private Image _combatPhase;
    [SerializeField] private Color _defaultPhaseColour;
    [SerializeField] private Color _activePhaseColour;
    [SerializeField] private Image _phasePopup;
    [SerializeField] private TextMeshProUGUI _phasePopupText;
    [SerializeField] private TextMeshProUGUI _phasePopupTurnText;

    [SerializeField] private GameObject _infoBox;
    [SerializeField] private TextMeshProUGUI _infoBoxText;
    [SerializeField] private GameObject _combatLog;
    [HideInInspector] private List<string> _combatLogValues = new List<string>();
    [SerializeField] private TextMeshProUGUI _combatLogText;

    public void Start()
    {
        FindAnyObjectByType<BoardController>().OnTileSelected += SetInfoBoxData;
        _combatLogText.text = string.Empty;
    }

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
                tileInfo += $"{ATTACK_COLOR_TAG}<b>Potential Damage:</b>{END_COLOR_TAG} {Creature.CalculateDamage(selectedCreatureTile.Occupant, occupant, selectedCreatureTile.Data)}\n";
            }
        }
        else
        {
            tileInfo += "<b>Occupant:</b> None\n";
        }
        _infoBoxText.text = tileInfo;
    }

    public void UpdateTurn(PlayerData player)
    {
        _turnText.text = $"Turn: {player.Name}";
    }

    public void UpdateEnergyCounter(int currentEnergy, int maxEnergy)
    {
        _energyCounter.text = FormatValue(currentEnergy, maxEnergy);
    }
    public void UpdateEnergyBonus(int energyBonus)
    {
        _energyBonusText.text = energyBonus > 0 ? $"+{energyBonus}" : string.Empty;
    }
    public void UpdateObjectiveCounter(int currentObjectives, int maxObjectives)
    {
        _objectiveCounter.text = FormatValue(currentObjectives, maxObjectives);
    }

    static string FormatValue(int value, int maxValue)
    {
        return $"{value}/{maxValue}";
    }

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
    public void UpdateCombatLogValues(AttackResult attackResult)
    {
        string logEntry = "";

        if (!attackResult.Attacker.Owner.Data.IsAI) logEntry += $"{PLAYER_COLOR_TAG}{attackResult.Attacker.Data.CreatureName}{END_COLOR_TAG} ";
        else logEntry += $"{ENEMY_COLOR_TAG}{attackResult.Attacker.Data.CreatureName}{END_COLOR_TAG} ";

        if (attackResult.IsDead) logEntry += "killed ";
        else logEntry += "attacked ";

        if (!attackResult.Defender.Owner.Data.IsAI) logEntry += $"{PLAYER_COLOR_TAG}{attackResult.Defender.Data.CreatureName}{END_COLOR_TAG} ";
        else logEntry += $"{ENEMY_COLOR_TAG}{attackResult.Defender.Data.CreatureName}{END_COLOR_TAG} ";

        if (attackResult.DamageDealt > 0 && !attackResult.IsDead) logEntry += $"for {attackResult.DamageDealt} damage.";
        else if (attackResult.DamageDealt == 0 && !attackResult.IsDead) logEntry += "but it was ineffective.";

        _combatLogValues.Add(logEntry);
        UpdateCombatLogDisplay();
    }

    private void UpdateCombatLogDisplay()
    {
        _combatLogText.text = string.Empty;
        foreach (string feedLine in _combatLogValues)
        {
            _combatLogText.text += feedLine + "\n";
        }
    }

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
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

    [SerializeField] private GameObject _tileInfo;
    [SerializeField] private TextMeshProUGUI _tileInfoText;
    [SerializeField] private GameObject _combatLog;
    [HideInInspector] private List<string> _combatLogValues = new List<string>();
    [SerializeField] private TextMeshProUGUI _combatLogText;

    public void Start()
    {
        FindAnyObjectByType<BoardController>().OnTileSelected += SetTileInfoData;
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

    public void SetTileInfoData(Tile tile)
    {
        if (tile == null)
        {
            _tileInfo.SetActive(false);
            return;
        }
        _tileInfo.SetActive(true);
        _tileInfo.transform.position = Mouse.current.position.ReadValue();
        string tileInfo = $"<b>Tile:</b> {tile.Data.Name}\n" +
            $"<b>Element:</b> {tile.Data.Element}\n" +
            $"<b>Elemental Resonance:</b> {tile.Data.EffectStrength}\n";

        Creature occupant = tile.Occupant;

        if (occupant != null)
        {
            tileInfo += $"<b>Occupant:</b> {occupant.Data.CreatureName}\n" +
                $"<b>Owner:</b> {occupant.Owner.Name}\n" +
                $"{HP_COLOR_TAG}<b>HP:</b>{END_COLOR_TAG} {FormatValue((int)occupant.CurrentHealth, (int)occupant.Data.MaxHealth)}\n" +
                $"{ATTACK_COLOR_TAG}<b>ATK:</b>{END_COLOR_TAG} {occupant.Data.Attack} {DEF_COLOR_TAG}<b>DEF:</b>{END_COLOR_TAG} {occupant.Data.Defense}\n";
        }
        else
        {
            tileInfo += "<b>Occupant:</b> None\n";
        }
        _tileInfoText.text = tileInfo;
    }

    public void UpdateTurn(PlayerData player)
    {
        _turnText.text = $"Turn: {player.Name}";
    }

    public void UpdateEnergyCounter(int currentEnergy, int maxEnergy)
    {
        _energyCounter.text = FormatValue(currentEnergy, maxEnergy);
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

        if (!attackResult.Attacker.Owner.IsAI) logEntry += $"{PLAYER_COLOR_TAG}{attackResult.Attacker.Data.CreatureName}{END_COLOR_TAG} ";
        else logEntry += $"{ENEMY_COLOR_TAG}{attackResult.Attacker.Data.CreatureName}{END_COLOR_TAG} ";

        if (attackResult.IsDead) logEntry += "killed ";
        else logEntry += "attacked ";

        if (!attackResult.Defender.Owner.IsAI) logEntry += $"{PLAYER_COLOR_TAG}{attackResult.Defender.Data.CreatureName}{END_COLOR_TAG} ";
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
        if (GUILayout.Button("Add Combat Log Entry (AttackResult)"))
        {
            //Dummy data
            Creature attacker = new GameObject("Attacker").AddComponent<Creature>();
            attacker.Data = new CreatureData { CreatureName = "Test Attacker" };
            PlayerData attackerOwner = new PlayerData { Name = "Player 1", IsAI = false };
            attacker.Owner = attackerOwner;
            Creature defender = new GameObject("Defender").AddComponent<Creature>();
            defender.Data = new CreatureData { CreatureName = "Test Defender" };
            PlayerData defenderOwner = new PlayerData { Name = "Player 2", IsAI = true };
            defender.Owner = defenderOwner;

            AttackResult attackResult = new AttackResult
            {
                Attacker = attacker,
                Defender = defender,
                DamageDealt = 10,
                IsDead = false
            };

            hud.UpdateCombatLogValues(attackResult);
        }
        if (GUILayout.Button("Show No Energy Popup"))
        {
            hud.ShowNoEnergyPopup();
        }
        if (GUILayout.Button("Randomise Turn Timer"))
        {
            hud.UpdateTurnTimer(UnityEngine.Random.Range(0f, 120f));
        }
    }
}
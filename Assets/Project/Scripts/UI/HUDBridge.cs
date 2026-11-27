using UnityEngine;
using UnityEngine.UI;

/*
 * HUDBridge feeds the match into Piotrs HUD the same way BattleAdapter sits
 * between the AI and the you guys code. Phase and turn update when the match state
 * changes energy and zones are checked every frame.
 */

public class HUDBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HUD _hud;
    [SerializeField] private Button _advancePhaseButton;

    private MatchController _match;
    private Board _board;
    private Graphic[] _advancePhaseGraphics;
    private bool? _advancePhaseActive;

    private void Awake()
    {
        if (_hud == null) _hud = FindAnyObjectByType<HUD>();
        if (_advancePhaseButton == null && _hud != null) _advancePhaseButton = FindChildButton("AdvancePhaseButton");
        if (_advancePhaseButton != null) _advancePhaseGraphics = _advancePhaseButton.GetComponentsInChildren<Graphic>(true);
        _match = FindAnyObjectByType<MatchController>();
        _board = FindAnyObjectByType<Board>();

        if (_hud == null || _match == null)
        {
            Debug.LogError("HUDBridge: needs a HUD and a MatchController in the scene.");
            enabled = false;
        }
    }

    private void OnEnable()
    {
        MatchController.OnMatchStateChanged += HandleStateChanged;
        Creature.OnAnyAttackImpact += HandleAttackImpact;
        Player.OnNotEnoughEnergy += HandleNotEnoughEnergy;
        PlayerHand.OnCannotAffordCard += HandleNotEnoughEnergyForCard;
    }

    private void OnDisable()
    {
        MatchController.OnMatchStateChanged -= HandleStateChanged;
        Creature.OnAnyAttackImpact -= HandleAttackImpact;
        Player.OnNotEnoughEnergy -= HandleNotEnoughEnergy;
        PlayerHand.OnCannotAffordCard -= HandleNotEnoughEnergyForCard;
    }

    private void Start()
    {
        if (_advancePhaseButton != null) _advancePhaseButton.onClick.AddListener(_match.RequestNextPhase);
    }

    private void Update()
    {
        Player localPlayer = FindLocalPlayer();

        if (localPlayer != null)
        {
            Resource energy = localPlayer.EnergySystem.Energy;
            _hud.UpdateEnergyCounter((int)energy.CurrentAmount, (int)energy.MaxAmount);
        }

        int totalZones = _board != null ? _board.ActiveZones.Count : 0;
        _hud.UpdateObjectiveCounter(ZonesHeldBy(localPlayer), totalZones);

        if (_advancePhaseButton != null)
        {
            bool playerTurn = _match.IsMatchActive && _match.CurrentPlayer != null && !_match.CurrentPlayer.Data.IsAI;
            _advancePhaseButton.interactable = playerTurn;
            SetAdvancePhaseTint(playerTurn);
        }
    }

    private void HandleStateChanged(MatchState state)
    {
        if (_match.CurrentPlayer != null) _hud.UpdateTurn(_match.CurrentPlayer.Data);

        if (state == MatchState.TurnSummon) _hud.UpdatePhase(true);
        else if (state == MatchState.TurnAction) _hud.UpdatePhase(false);
    }

    private void HandleAttackImpact(AttackResult result) => _hud.UpdateCombatLogValues(result);

    private Player FindLocalPlayer()
    {
        foreach (Player player in PlayerRegistry.MasterList)
            if (player != null && !player.Data.IsAI) return player;
        return null;
    }

    private int ZonesHeldBy(Player player)
    {
        if (_board == null || player == null) return 0;
        int count = 0;
        foreach (ZoneInstance zone in _board.ActiveZones)
            if (zone.Occupier == player) count++;
        return count;
    }

    private void SetAdvancePhaseTint(bool active)
    {
        if (_advancePhaseActive == active) return;
        _advancePhaseActive = active;

        Color tint = active ? Color.white : _advancePhaseButton.colors.disabledColor;
        foreach (Graphic graphic in _advancePhaseGraphics)
            if (graphic != _advancePhaseButton.targetGraphic) graphic.CrossFadeColor(tint, 0.1f, true, true);
    }

    private Button FindChildButton(string objectName)
    {
        foreach (Button button in _hud.GetComponentsInChildren<Button>(true))
            if (button.name == objectName) return button;
        return null;
    }

    private void HandleNotEnoughEnergy(Player player)
    {
        if (!player.Data.IsAI) _hud.ShowNoEnergyPopup();
    }

    private void HandleNotEnoughEnergyForCard() => _hud.ShowNoEnergyPopup();
}
